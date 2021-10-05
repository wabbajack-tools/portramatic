using System.IO;
using System.Reactive.Disposables;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Portramatic.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Portramatic.Views
{
    public partial class GalleryItem : ReactiveUserControl<GalleryItemViewModel>
    {
        public GalleryItem()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.CompressedImage, view => view.GalleryImage.Source,
                    bytes => new Bitmap(new MemoryStream(bytes))
                        .DisposeWith(disposables));
                this.BindCommand(ViewModel, vm => vm.Clicked, view => view.FocusButton)
                    .DisposeWith(disposables);
            });
        }
    }
}