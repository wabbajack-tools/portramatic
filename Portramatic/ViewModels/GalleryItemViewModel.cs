using System.Reactive;
using Avalonia.Media;
using Portramatic.DTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Paths.IO;

namespace Portramatic.ViewModels;

public class GalleryItemViewModel
{
    private readonly MainWindowViewModel _mwvm;

    public GalleryItemViewModel(MainWindowViewModel mwvm)
    {
        _mwvm = mwvm;
        Clicked = ReactiveCommand.Create(() =>
        {
            _mwvm.Focus(Definition);
        });
    }
    
    [Reactive]
    public PortraitDefinition Definition { get; set; }
    
    [Reactive]
    public byte[] CompressedImage { get; set; }
    
    [Reactive]
    public ReactiveCommand<Unit, Unit> Clicked { get; set; }


    public void Delete()
    {
        var hex = Definition.MD5;
        var path = Program.AdminPath.Combine(hex[..2], hex, "definition.json");
        if (path.FileExists())
            path.Delete();
    }
}