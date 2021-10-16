using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DynamicData;
using Portramatic.DTOs;
using Portramatic.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;
using SkiaSharp;

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

        [Reactive] public PortraitDefinitionViewModel Definition { get; set; }
        
        [Reactive]
        public ReactiveCommand<Unit, Unit> Export { get; set; }
        
        [Reactive]
        public ReactiveCommand<Unit, Unit> Install { get; set; }

        [Reactive] public int CurrentTab { get; set; } = 0;

        [Reactive] public string SearchTags { get; set; } = "";
        
        public SourceCache<GalleryItemViewModel, string> _galleryItems = new(vm => vm.Definition.MD5);

        private readonly ReadOnlyObservableCollection<GalleryItemViewModel> _data;

        public ReadOnlyObservableCollection<GalleryItemViewModel> GalleryItems => _data;

        public MainWindowViewModel()
        {
            Definition = new();
            Activator = new ViewModelActivator();
            _client = new HttpClient();

            LoadGallery();

            Export = ReactiveCommand.Create(() =>
            {
                DoExport("output");
            });

            Install = ReactiveCommand.Create(() =>
            {
                InstallFiles();
            });

            var filterFunction = this.WhenAnyValue(vm => vm.SearchTags)
                .Select<string, Func<GalleryItemViewModel, bool>>(s =>
                {
                    if (s == "") return itm => true;
                    var split = s.Replace(",", "").Split(" ",
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    return itm => split.Any(s => itm.Definition.Tags.Contains(s));
                });

            _galleryItems.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Filter(filterFunction)
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
                    .SelectAsync(disposables, async url =>
                    {
                        if (url.Scheme == "file") 
                            return await File.ReadAllBytesAsync(HttpUtility.UrlDecode(url.AbsolutePath));
                        return await _client.GetByteArrayAsync(url);
                    })
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

        private async Task InstallFiles()
        {
            var wotrFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"..\LocalLow\Owlcat Games\Pathfinder Wrath Of The Righteous");
            if (Directory.Exists(wotrFolder))
            {
                await DoExport(Path.Combine(wotrFolder, "Portraits"));
            }
            
            var kingmakerFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"..\LocalLow\Owlcat Games\Pathfinder Kingmaker");
            if (Directory.Exists(kingmakerFolder))
            {
                await DoExport(Path.Combine(kingmakerFolder, "Portraits"));
            }
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
                    l.AddOrUpdate(new GalleryItemViewModel(this)
                    {
                        Definition = d,
                        CompressedImage = datas[d.MD5]
                    } );
                }
            });
            return;
        }

        private async Task DoExport(string baseFolder)
        {
            baseFolder = Path.Combine(baseFolder, Definition.MD5);
            Directory.CreateDirectory(baseFolder);
            
            var definition = Definition.AsDTO();
            await ExportImage(baseFolder, definition, ImageSize.Small);
            await ExportImage(baseFolder, definition, ImageSize.Medium);
            await ExportImage(baseFolder, definition, ImageSize.Full);
            await ExportDefinition(baseFolder, definition);
        }

        private async Task ExportDefinition(string baseFolder, PortraitDefinition definition)
        {
            var outPath = Path.Combine(baseFolder, "definition.json");

            var json = JsonSerializer.Serialize(definition, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            await File.WriteAllTextAsync(outPath, json);
            if (Definition.Source.Scheme != "file")
            {
                await _client.PostAsync("https://portramatic.wabbajack.workers.dev", new StringContent(json));
            }
        }

        private async Task ExportImage(string baseFolder, PortraitDefinition definition, ImageSize size)
        {
            var image = SKImage.FromEncodedData(ImageData);
            var cropData = definition.CropData(size);
            var cropped = definition.Crop(image, size);
            var outPath = Path.Combine(baseFolder, cropData.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
            var data = cropped.Encode(SKEncodedImageFormat.Png, 100);
            await using var fstream = File.Open(outPath, FileMode.Create, FileAccess.Write);
            data.SaveTo(fstream);
        }

        public static string CreateMD5(byte[] input)
        {
            // Use input string to calculate MD5 hash
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hashBytes = md5.ComputeHash(input);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public async Task Focus(PortraitDefinition definition)
        {
            CurrentTab = 1;
            var data = await _client.GetByteArrayAsync(definition.Source);
            ImageData = data;
            //await Task.Delay(500);
            Definition.Load(definition);
            Url = Definition.Source.ToString();
        }
    }
}
