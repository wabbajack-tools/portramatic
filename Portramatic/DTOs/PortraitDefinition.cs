using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media;
using OpenCvSharp.DnnSuperres;
using OpenCvSharp;
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

    public async Task<SKImage> Crop(SKImage src, ImageSize size)
    {
        src = await Upscale(src);

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

    private static Task<DnnSuperResImpl> Dnn = Task.Run(async () =>
    {
        var model = new DnnSuperResImpl("fsrcnn", 4);
        using var modelFile = await TempFile.Resource("Portramatic.Resources.FSRCNN_x4.pb");
        model.ReadModel(modelFile.Name);
        return model;
    });
    public async Task<SKImage> Upscale(SKImage src)
    {
        using var tmpSrc = new TempFile(".png");
        using var tmpSrc2 = new TempFile(".png");
        using (var os = File.Create(tmpSrc.Name))
        {
            src.Encode(SKEncodedImageFormat.Png, 100).SaveTo(os);
        }

        var mat = new Mat(tmpSrc.Name, ImreadModes.Color);
        var matOut = new Mat();
        (await Dnn).Upsample(mat, matOut);
        matOut.SaveImage(tmpSrc2.Name);
        return SKImage.FromEncodedData(tmpSrc2.Name);
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