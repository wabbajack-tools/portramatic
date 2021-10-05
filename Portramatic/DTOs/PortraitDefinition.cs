using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media;
using SkiaSharp;

namespace Portramatic.DTOs;


public enum ImageSize
{
    Small,
    Medium,
    Full
}

public class CroppedImage
{
    [JsonPropertyName("scale")]
    public double Scale { get; set; }
    
    [JsonPropertyName("offset_x")]
    public double OffsetX { get; set; }
    
    [JsonPropertyName("offset_y")]
    public double OffsetY { get; set; }
    
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


public class PortraitDefinition
{
    [JsonPropertyName("source")] public Uri Source { get; set; }

    [JsonPropertyName("md5")] public string MD5 { get; set; }

    [JsonPropertyName("tags")] public string[] Tags { get; set; }

    [JsonPropertyName("small")] public CroppedImage Small { get; set; } = new() { Size = ImageSize.Small };

    [JsonPropertyName("medium")] public CroppedImage Medium { get; set; } = new() { Size = ImageSize.Medium };

    [JsonPropertyName("full")] public CroppedImage Full { get; set; } = new() { Size = ImageSize.Full };

    [JsonPropertyName("version")] public int Version => 2;

    public SKImage Crop(SKImage src, ImageSize size)
    {

        var cropData = CropData(size);

        var (width, height) = cropData.FinalSize;

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        surface.Canvas.Translate((float)cropData.OffsetX, (float)cropData.OffsetY);
        surface.Canvas.Scale((float)cropData.Scale, (float)cropData.Scale);

        var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.FilterQuality = SKFilterQuality.High;
        surface.Canvas.DrawImage(src, new SKPoint(0, 0), paint);
        return surface.Snapshot();
    }

    public CroppedImage CropData(ImageSize size)
    {
        return size switch
        {
            ImageSize.Small => Small,
            ImageSize.Medium => Medium,
            ImageSize.Full => Full,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}