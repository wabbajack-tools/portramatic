using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using Portramatic.ViewModels;
using ReactiveUI;

namespace Portramatic.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.GalleryItems, view => view.Gallery.Items)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.Url, view => view.UrlBox.Text)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.Definition.MD5, view => view.ImageHash.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.RawImageViewSmall.Source)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Width, view => view.RawImageViewSmall.Width)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Height, view => view.RawImageViewSmall.Height)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.RawImageViewMedium.Source)
                    .DisposeWith(disposables);
                
                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Width, view => view.RawImageViewMedium.Width)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Height, view => view.RawImageViewMedium.Height)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.RawImageViewFull.Source)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Width, view => view.RawImageViewFull.Width)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RawImage.Size.Height, view => view.RawImageViewFull.Height)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.Export, view => view.ExportButton)
                    .DisposeWith(disposables);

                this.PAZControlSmall.WhenAnyValue(view => view.ZoomX)
                    .BindTo(ViewModel, vm => vm.Definition.Small.Scale)
                    .DisposeWith(disposables);
                
                this.PAZControlSmall.WhenAnyValue(view => view.OffsetX)
                    .BindTo(ViewModel, vm => vm.Definition.Small.OffsetX)
                    .DisposeWith(disposables);
                
                this.PAZControlSmall.WhenAnyValue(view => view.OffsetY)
                    .BindTo(ViewModel, vm => vm.Definition.Small.OffsetY)
                    .DisposeWith(disposables);

                
                this.PAZControlMedium.WhenAnyValue(view => view.ZoomX)
                    .BindTo(ViewModel, vm => vm.Definition.Medium.Scale)
                    .DisposeWith(disposables);
                
                this.PAZControlMedium.WhenAnyValue(view => view.OffsetX)
                    .BindTo(ViewModel, vm => vm.Definition.Medium.OffsetX)
                    .DisposeWith(disposables);
                
                this.PAZControlMedium.WhenAnyValue(view => view.OffsetY)
                    .BindTo(ViewModel, vm => vm.Definition.Medium.OffsetY)
                    .DisposeWith(disposables);

                
                this.PAZControlFull.WhenAnyValue(view => view.ZoomX)
                    .BindTo(ViewModel, vm => vm.Definition.Full.Scale)
                    .DisposeWith(disposables);
                
                this.PAZControlFull.WhenAnyValue(view => view.OffsetX)
                    .BindTo(ViewModel, vm => vm.Definition.Full.OffsetX)
                    .DisposeWith(disposables);
                
                this.PAZControlFull.WhenAnyValue(view => view.OffsetY)
                    .BindTo(ViewModel, vm => vm.Definition.Full.OffsetY)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.Definition.Tags, view => view.Tags.Text,
                        p => string.Join(", ", p ?? Array.Empty<string>()),
                        s => s.Split(",").Select(s => s.Trim()).ToArray())
                    .DisposeWith(disposables);

            });
        }
    }
}