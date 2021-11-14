using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
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

                ResaveButton.IsVisible = Program.IsAdminMode;
                this.OneWayBind(ViewModel, vm => vm.GalleryItems, view => view.Gallery.Items)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.Url, view => view.UrlBox.Text)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.Definition.MD5, view => view.ImageHash.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.Small.Source)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.Definition.Small.Scale)
                    .CombineLatest(ViewModel.WhenAnyValue(vm => vm.Definition.Small.OffsetX),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Small.OffsetY),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Small.Rotation),
                        ViewModel.WhenAnyValue(vm => vm.RawImage))
                    .Where(d => d.Fifth != null)
                    .Subscribe(t =>
                    {
                        var matrix =
                            Matrix.CreateRotation(t.Fourth) *
                            Matrix.CreateScale(t.First, t.First) *
                            Matrix.CreateTranslation(t.Second, t.Third);

                        Small.RenderTransformOrigin = RelativePoint.TopLeft;
                        Small.RenderTransform = new MatrixTransform(matrix);

                    }).DisposeWith(disposables);
                    
/*
                ViewModel.WhenAnyValue(vm => vm.Definition.Small.Scale)
                    .CombineLatest(ViewModel.WhenAnyValue(vm => vm.RawImage))
                    .Where(d => d.Second != null)
                    .Subscribe(t =>
                    {
                        Small.Width = t.Second.Size.Width * t.First;
                        Small.Height = t.Second.Size.Height * t.First;
                    })
                    .DisposeWith(disposables);
                
                ViewModel.WhenAnyValue(vm => vm.Definition.Small.OffsetX)
                    .CombineLatest(ViewModel.WhenAnyValue(vm => vm.Definition.Small.OffsetY))
                    //.Throttle(TimeSpan.FromMilliseconds(100))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(t =>
                    {
                        Canvas.SetLeft(Small, t.First);
                        Canvas.SetTop(Small, t.Second);
                    }).DisposeWith(disposables);
                    */

                
                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.Medium.Source)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.Definition.Medium.Scale)
                    .CombineLatest(ViewModel.WhenAnyValue(vm => vm.Definition.Medium.OffsetX),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Medium.OffsetY),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Medium.Rotation),
                        ViewModel.WhenAnyValue(vm => vm.RawImage))
                    .Where(d => d.Fifth != null)
                    .Subscribe(t =>
                    {
                        var matrix =
                            Matrix.CreateRotation(t.Fourth) *
                            Matrix.CreateScale(t.First, t.First) *
                            Matrix.CreateTranslation(t.Second, t.Third);

                        Medium.RenderTransformOrigin = RelativePoint.TopLeft;
                        Medium.RenderTransform = new MatrixTransform(matrix);

                    }).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.RawImage, view => view.Full.Source)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.Definition.Full.Scale)
                    .CombineLatest(ViewModel.WhenAnyValue(vm => vm.Definition.Full.OffsetX),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Full.OffsetY),
                        ViewModel.WhenAnyValue(vm => vm.Definition.Full.Rotation),
                        ViewModel.WhenAnyValue(vm => vm.RawImage))
                    .Where(d => d.Fifth != null)
                    .Subscribe(t =>
                    {

                        var matrix =
                            Matrix.CreateRotation(t.Fourth) *
                            Matrix.CreateScale(t.First, t.First) *
                            Matrix.CreateTranslation(t.Second, t.Third);

                        Full.RenderTransformOrigin = RelativePoint.TopLeft;
                        Full.RenderTransform = new MatrixTransform(matrix);

                    }).DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.Export, view => view.ExportButton)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.Install, view => view.InstallButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.CurrentTab, view => view.TabControl.SelectedIndex)
                    .DisposeWith(disposables);
                

                this.Bind(ViewModel, vm => vm.Definition.Tags, view => view.Tags.Text,
                        p => string.Join(", ", p ?? Array.Empty<string>()),
                        s => s.Split(",").Select(s => s.Trim()).ToArray())
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.SearchTags, view => view.SearchTags.Text)
                    .DisposeWith(disposables);
            });
        }

        private void InputElement_OnPointerWheelChangedSmall(object? sender, PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                ViewModel!.Definition.Small.Rotation += (e.Delta.Y * 0.01);
            }
            else
            {
                ViewModel!.Definition.Small.Scale += (e.Delta.Y * 0.01);
            }

        }
        private void InputElement_OnPointerWheelChangedMedium(object? sender, PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                ViewModel!.Definition.Medium.Rotation += (e.Delta.Y * 0.01);
            }
            else
            {
                ViewModel!.Definition.Medium.Scale += (e.Delta.Y * 0.01);
            }
        }
        private void InputElement_OnPointerWheelChangedFull(object? sender, PointerWheelEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                ViewModel!.Definition.Full.Rotation += (e.Delta.Y * 0.01);
            }
            else
            {
                ViewModel!.Definition.Full.Scale += (e.Delta.Y * 0.01);
            }
        }

        private Vector? _lastPositionSmall = null;
        private void InputElement_OnPointerMovedSmall(object? sender, PointerEventArgs e)
        {
            if (_lastPositionSmall != null)
            {
                var pos = e.GetPosition(SmallCanvas);
                var delta =  pos - _lastPositionSmall;
                if (!delta.HasValue) return;
                _lastPositionSmall = pos;
                
                
                ViewModel!.Definition.Small.OffsetX += delta.Value.X;
                ViewModel!.Definition.Small.OffsetY += delta.Value.Y;
            }
        }
        private void InputElement_OnPointerPressedSmall(object? sender, PointerPressedEventArgs e)
        {
            _lastPositionSmall = e.GetPosition(SmallCanvas);
        }
        private void InputElement_OnPointerReleasedSmall(object? sender, PointerReleasedEventArgs e)
        {
            _lastPositionSmall = null;
        }
        
        private Vector? _lastPositionMedium = null;
        private void InputElement_OnPointerMovedMedium(object? sender, PointerEventArgs e)
        {
            if (_lastPositionMedium != null)
            {
                var pos = e.GetPosition(MediumCanvas);
                var delta =  pos - _lastPositionMedium;
                if (!delta.HasValue) return;
                _lastPositionMedium = pos;
                
                
                ViewModel!.Definition.Medium.OffsetX += delta.Value.X;
                ViewModel!.Definition.Medium.OffsetY += delta.Value.Y;
            }
        }
        private void InputElement_OnPointerPressedMedium(object? sender, PointerPressedEventArgs e)
        {
            _lastPositionMedium = e.GetPosition(MediumCanvas);
        }
        private void InputElement_OnPointerReleasedMedium(object? sender, PointerReleasedEventArgs e)
        {
            _lastPositionMedium = null;
        }
        
        
        private Vector? _lastPositionFull = null;
        private void InputElement_OnPointerMovedFull(object? sender, PointerEventArgs e)
        {
            if (_lastPositionFull != null)
            {
                var pos = e.GetPosition(FullCanvas);
                var delta =  pos - _lastPositionFull;
                if (!delta.HasValue) return;
                _lastPositionFull = pos;
                
                
                ViewModel!.Definition.Full.OffsetX += delta.Value.X;
                ViewModel!.Definition.Full.OffsetY += delta.Value.Y;
            }

        }
        private void InputElement_OnPointerPressedFull(object? sender, PointerPressedEventArgs e)
        {
            _lastPositionFull = e.GetPosition(FullCanvas);
        }
        private void InputElement_OnPointerReleasedFull(object? sender, PointerReleasedEventArgs e)
        {
            _lastPositionFull = null;
        }

        private void FromDisk_OnClick(object? sender, RoutedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var dialog = new OpenFileDialog()
                {
                    Title = "Select a modlist.txt file",
                    AllowMultiple = false
                };
                var result = await dialog.ShowAsync(this);
                if (result is { Length: > 0 })
                {
                    ViewModel!.Url = result.First();
                }
            });

        }

        private void ResetTransforms(object? sender, RoutedEventArgs e)
        {
            ViewModel!.Definition.Small.Reset();
            ViewModel!.Definition.Medium.Reset();
            ViewModel!.Definition.Full.Reset();
        }

        private void Resave(object? sender, RoutedEventArgs e)
        {
            ViewModel!.Resave();
        }
    }
}