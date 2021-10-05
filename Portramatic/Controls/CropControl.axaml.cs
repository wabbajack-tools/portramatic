using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Portramatic.Controls
{
    public partial class CropControl : ReactiveUserControl<CropControlViewModel>
    {
        [Reactive]
        public IImage Image { get; set; }
        
        public CropControl()
        {
            InitializeComponent();
            
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(view => view.Image)
                    .Where(v => v != null)
                    .BindTo(this, view => view.CurrentImage.Source)
                    .DisposeWith(disposables);

            });
        }
    }
}