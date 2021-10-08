using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Portramatic.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Portramatic.Views
{
    public partial class GalleryItem : ReactiveUserControl<GalleryItemViewModel>
    {
        public static Lazy<Bitmap> DefaultBitmap = new Lazy<Bitmap>(() =>
        {
            var _image = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Portramatic.Resources.BlankImage.png");
            return new Bitmap(_image);
        });
        
        public GalleryItem()
        {
            InitializeComponent();

            GalleryImage.Source = DefaultBitmap.Value;
            
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.CompressedImage, view => view.GalleryImage.Source,
                    bytes => new Bitmap(new MemoryStream(bytes))
                        .DisposeWith(new CompositeDisposable(disposables, Disposable.Create(() => GalleryImage.Source = DefaultBitmap.Value))));
                this.BindCommand(ViewModel, vm => vm.Clicked, view => view.FocusButton)
                    .DisposeWith(disposables);
            });
        }
    }
}