using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using DynamicData;
using DynamicData.Binding;
using Portramatic.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;
using SkiaSharp;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Portramatic.ViewModels
{

    public class MainWindowViewModel : ReactiveValidationObject, IActivatableViewModel
    {
        private readonly HttpClient _client;

        [Reactive]
        public string Url { get; set; } = "";
        
        [Reactive]
        public IImage RawImage { get; set; }
        
        [Reactive]
        public byte[] ImageData { get; set; }
        
        public ViewModelActivator Activator { get; protected set; }

        [Reactive] public PortraitDefinition Definition { get; set; } = new();
        
        [Reactive]
        public ReactiveCommand<Unit, Unit> Export { get; set; }

        public SourceCache<GalleryItemViewModel, string> _galleryItems = new(vm => vm.Definition.MD5);

        private readonly ReadOnlyObservableCollection<GalleryItemViewModel> _data;

        public ReadOnlyObservableCollection<GalleryItemViewModel> GalleryItems => _data;

        public MainWindowViewModel()
        {
            Activator = new ViewModelActivator();
            _client = new HttpClient();

            LoadGallery();

            Export = ReactiveCommand.Create(() =>
            {
                DoExport();
            });

            _galleryItems.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _data)
                .Subscribe();
            
            this.WhenActivated(disposables =>
            {

                
                
                this.WhenAnyValue(vm => vm.Url)
                    .Select(v => (Uri.TryCreate(v, UriKind.Absolute, out var url), url))
                    .Where(v => v.Item1)
                    .Select(v => v.url)
                    .BindTo(this, vm => vm.Definition.Source)
                    .DisposeWith(disposables);

                this.WhenAnyValue(vm => vm.Definition.Source)
                    .Where(u => u != default)
                    .SelectAsync(disposables, async url => await _client.GetByteArrayAsync(url))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .BindTo(this, vm => vm.ImageData)
                    .DisposeWith(disposables);

                this.WhenAnyValue(vm => vm.ImageData)
                    .Where(u => u != default)
                    .Select(u => new Bitmap(new MemoryStream(u)))
                    .BindTo(this, vm => vm.RawImage)
                    .DisposeWith(disposables);

                this.WhenAnyValue(vm => vm.ImageData)
                    .Where(u => u != default)
                    .Select(CreateMD5)
                    .BindTo(this, vm => vm.Definition.MD5)
                    .DisposeWith(disposables);

            });
            
        }

        private void LoadGallery()
        {
            var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"Portramatic.Resources.gallery.zip");

            PortraitDefinition[] definitions;
            using var zip = new ZipArchive(resourceStream!, ZipArchiveMode.Read, false);
            
                using var definitionStream = zip.GetEntry("definitions.json")!.Open();
                definitions = JsonSerializer.Deserialize<PortraitDefinition[]>(definitionStream,
                    new JsonSerializerOptions()
                    {
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                        AllowTrailingCommas = true
                    })!;

            var datas = zip.Entries
                .Where(e => e.Name.EndsWith(".webp"))
                .Select(e => 
            {
                var ms = new MemoryStream();
                using var es = e.Open();
                es.CopyTo(ms);
                return (Path.GetFileNameWithoutExtension(e.Name), ms.ToArray());
            }).ToDictionary(e => e.Item1, e => e.Item2);

            _galleryItems.Edit(l =>
            {
                l.Clear();
                foreach (var d in definitions)
                {
                    l.AddOrUpdate(new GalleryItemViewModel
                    {
                        Definition = d,
                        CompressedImage = datas[d.MD5]
                    } );
                }
            });
            return;
        }

        private async Task DoExport()
        {
            await ExportImage(Definition, Definition.Small);
            await ExportImage(Definition, Definition.Medium);
            await ExportImage(Definition, Definition.Full);
            await ExportDefinition(Definition);
        }

        private async Task ExportDefinition(PortraitDefinition definition)
        {
            var outPath = Path.Combine("output", definition.MD5[..4], definition.MD5, "definition.json");
            
            var json = definition.ToJSON();
            var pend = _client.PostAsync("https://portramatic.wabbajack.workers.dev", new StringContent(json));
            await File.WriteAllTextAsync(outPath, json);
            await pend;
        }

        private async Task ExportImage(PortraitDefinition definition, CroppedImage cropData)
        {
            Directory.CreateDirectory("output");
            var (width, height) = cropData.FinalSize;
            
            var outputImage = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Opaque);
           
            using (var locked = outputImage.Lock())
            {
                var info = new SKImageInfo(locked.Size.Width, locked.Size.Height, locked.Format.ToSkColorType(), SKAlphaType.Opaque);
                using (var surface = SKSurface.Create(info, locked.Address, locked.RowBytes))
                {
                    using var src = SKImage.FromEncodedData(new MemoryStream(ImageData));
                    
                    surface.Canvas.Translate((float)-(RawImage.Size.Width / 2), (float)-(RawImage.Size.Height / 2));
                    surface.Canvas.Translate((float)cropData.OffsetX, (float)cropData.OffsetY);
                    surface.Canvas.Translate((float)width/2, (float)height/2);
                    surface.Canvas.Scale((float)cropData.Scale, (float)cropData.Scale);
                    
                    
                    surface.Canvas.DrawImage(src, new SKPoint(0, 0));
                }

            }

            var outPath = Path.Combine("output", definition.MD5[..4], definition.MD5, cropData.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            outputImage.Save(outPath);


            return;
        }

        public static string CreateMD5(byte[] input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(input);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
