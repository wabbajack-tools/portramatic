using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Portramatic.DTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Portramatic.ViewModels;

public class CroppedImageViewModel : ViewModelBase
{
    [Reactive] [JsonPropertyName("scale")] public double Scale { get; set; } = 1;

    [Reactive]
    [JsonPropertyName("offset_x")]
    public double OffsetX { get; set; }
    
    [Reactive]
    [JsonPropertyName("offset_y")]
    public double OffsetY { get; set; }
    
    [Reactive]
    [JsonPropertyName("size")]
    public ImageSize Size { get; set; }

    [JsonIgnore]
    public (int Width, int Height) FinalSize =>
        Size switch
        {
            ImageSize.Full => (692, 1024),
            ImageSize.Medium => (330, 432),
            ImageSize.Small => (185, 242),
            _ => throw new ArgumentOutOfRangeException()
        };

    [JsonIgnore]
    public string FileName =>
        Size switch
        {
            ImageSize.Full => "Fulllength.png",
            ImageSize.Medium => "Medium.png",
            ImageSize.Small => "Small.png",
            _ => throw new ArgumentOutOfRangeException()
        };

    public void Load(CroppedImage dto)
    {
        Scale = dto.Scale;
        OffsetX = dto.OffsetX;
        OffsetY = dto.OffsetY;
        Size = dto.Size;
    }

    public CroppedImage AsDTO()
    {
        return new CroppedImage
        {
            Scale = Scale,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Size = Size
        };
    }
}


public class PortraitDefinitionViewModel : ViewModelBase
{
    [Reactive]
    [JsonPropertyName("source")]
    public Uri Source { get; set; }
    
    [Reactive]
    [JsonPropertyName("md5")]
    public string MD5 { get; set; }
    
    [Reactive]
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    [Reactive]
    [JsonPropertyName("small")] 
    public CroppedImageViewModel Small { get; set; } = new() {Size = ImageSize.Small};

    [Reactive]
    [JsonPropertyName("medium")] 
    public CroppedImageViewModel Medium { get; set; } = new() {Size = ImageSize.Medium};

    [Reactive]
    [JsonPropertyName("full")] 
    public CroppedImageViewModel Full { get; set; } = new() { Size = ImageSize.Full };
    
    
    public void Load(PortraitDefinition dto)
    {
        Source = dto.Source;
        Tags = dto.Tags;
        Small.Load(dto.Small);
    }

    public PortraitDefinition AsDTO()
    {
        return new PortraitDefinition
        {
            Source = Source,
            MD5 = MD5,
            Tags = Tags,
            Small = Small.AsDTO(),
            Medium = Medium.AsDTO(),
            Full = Full.AsDTO()
        };
    }
}