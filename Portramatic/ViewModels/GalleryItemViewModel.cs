using Avalonia.Media;
using ReactiveUI.Fody.Helpers;

namespace Portramatic.ViewModels;

public class GalleryItemViewModel
{
    [Reactive]
    public PortraitDefinition Definition { get; set; }
    
    [Reactive]
    public byte[] CompressedImage { get; set; }
}