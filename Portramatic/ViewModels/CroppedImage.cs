using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Portramatic.ViewModels;

public enum ImageSize
{
    Small,
    Medium,
    Full
}

public class CroppedImage : ViewModelBase
{
    [Reactive]
    [JsonPropertyName("scale")]
    public double Scale { get; set; }
    
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

}


public class PortraitDefinition : ViewModelBase
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
    public CroppedImage Small { get; set; } = new() {Size = ImageSize.Small};

    [Reactive]
    [JsonPropertyName("medium")] 
    public CroppedImage Medium { get; set; } = new() {Size = ImageSize.Medium};

    [Reactive]
    [JsonPropertyName("full")] 
    public CroppedImage Full { get; set; } = new() { Size = ImageSize.Full };
    
    
    public string ToJSON()
    {
        var obj = new
        {
            source = Source,
            md5 = MD5,
            tags = Tags,
            small = new
            {
                scale = Small.Scale,
                offset_x = Small.OffsetX,
                offset_y = Small.OffsetY,
                size = Small.Size,
            },
            medium = new
            {
                scale = Medium.Scale,
                offset_x = Medium.OffsetX,
                offset_y = Medium.OffsetY,
                size = Medium.Size,
            },
            full = new
            {
                scale = Full.Scale,
                offset_x = Full.OffsetX,
                offset_y = Full.OffsetY,
                size = Full.Size,
            }
        };

        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            },
            WriteIndented = true,
            IgnoreReadOnlyFields = true
        });
        return json;
    }
}