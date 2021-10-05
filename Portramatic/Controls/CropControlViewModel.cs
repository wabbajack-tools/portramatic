using Avalonia.Media;
using Portramatic.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Portramatic.Controls;

public class CropControlViewModel : CroppedImageViewModel, IActivatableViewModel
{
    [Reactive]
    public IImage Image { get; set; }
    
    [Reactive]
    public double ScaleX { get; set; }
    public ViewModelActivator Activator { get; }

    public CropControlViewModel()
    {
        Activator = new ViewModelActivator();
    }
}