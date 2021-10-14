using Avalonia.Media.Imaging;
using Shipwreck.Phash.Imaging;
using SkiaSharp;

namespace ThumbnailGenerator;

public class SKImageIBitmap : IByteImage
{
    private readonly SKBitmap _bmp;
    public int Width => _bmp.Width;
    public int Height => _bmp.Height;

    public byte this[int x, int y]
    {
        get
        {
            var px = _bmp.GetPixel(x, y);
            px.ToHsl(out var h, out var s, out var l);
            return (byte)(l * 255f);
        }
    }

    public SKImageIBitmap(SKImage img)
    {
        _bmp = new SKBitmap(new SKImageInfo(512, 512, SKColorType.Bgra8888));
        SKBitmap.FromImage(img).ScalePixels(_bmp, SKFilterQuality.High);
    }
}