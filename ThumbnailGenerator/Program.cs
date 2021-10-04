// See https://aka.ms/new-console-template for more information

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Portramatic.ViewModels;
using SkiaSharp;

class Program
{
    private static HttpClient Client = new();
    public static async Task<int> Main(string[] args)
    {
        var files = Directory.EnumerateFiles(Path.Combine(args[0], "Definitions"), "definition.json",
                SearchOption.AllDirectories)
            .ToArray();
        
        Console.WriteLine($"Found {files.Length} definitions, loading...");

        var definitions = new List<PortraitDefinition>();

        foreach (var file in files)
        {
            definitions.Add(JsonSerializer.Deserialize<PortraitDefinition>(await File.ReadAllTextAsync(file), new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            })!);
        }
        
        Console.WriteLine($"Loaded {definitions.Count} definitions, creating gallery files");

        var outputMemoryStream = new MemoryStream();
        {
            using var archive = new ZipArchive(outputMemoryStream, ZipArchiveMode.Create, true);
            {
                {
                    var tocEntry = archive.CreateEntry("definitions.json", CompressionLevel.SmallestSize);
                    await using var tocStream = tocEntry.Open();
                    await using var sw = new StreamWriter(tocStream);
                    await sw.WriteLineAsync("[");
                    foreach (var definition in definitions)
                    {
                        await sw.WriteAsync(definition.ToJSON());
                        await sw.WriteLineAsync(",");
                    }

                    await sw.WriteLineAsync("]");
                }
                foreach (var (definition, idx) in definitions.Select((v, idx) => (v, idx)))
                {
                    Console.WriteLine($"[{idx}/{definitions.Count}]Adding {definition.Source.ToString().Substring(0, Math.Min(70, definition.Source.ToString().Length))}");
                    await GenerateThumbnail(archive, definition);
                }
            }
        }
        Console.WriteLine($"Output zip is {outputMemoryStream.Length} bytes in size");
        await File.WriteAllBytesAsync(Path.Combine(args[0], "Definitions", "gallery.zip"), outputMemoryStream.ToArray());
        return 0;
    }

    private static async Task GenerateThumbnail(ZipArchive archive, PortraitDefinition definition)
    {
        var bitmapBytes = await Client.GetByteArrayAsync(definition.Source);
        var (width, height) = definition.Full.FinalSize;
        var cropData = definition.Full;
        
        using (var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque)))
        {
            using var src = SKImage.FromEncodedData(new MemoryStream(bitmapBytes));
                    
            surface.Canvas.Translate((float)-(src.Width / 2), (float)-(src.Height / 2));
            surface.Canvas.Translate((float)cropData.OffsetX, (float)cropData.OffsetY);
            surface.Canvas.Translate((float)width/2, (float)height/2);
            surface.Canvas.Scale((float)cropData.Scale, (float)cropData.Scale);
                    
                    
            surface.Canvas.DrawImage(src, new SKPoint(0, 0));
            var snap = Resize(surface.Snapshot(), width / 4, height / 4);
            
            
            var encoded = snap.Encode(SKEncodedImageFormat.Webp, 50);
            
            lock (archive)
            { 
                var entry = archive.CreateEntry(definition.MD5+".webp");
                using var entryStream = entry.Open();
                encoded.SaveTo(entryStream);
            }
        }

    }

    private static SKImage Resize(SKImage i, int width, int height)
    {
        using var surface = SKSurface.Create(new SKImageInfo(i.Width/4, i.Height/4, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.DrawImage(i, new SKRect(0, 0, i.Width, i.Height), new SKRect(0, 0, (float)i.Width/4, (float)i.Height/4));
        return surface.Snapshot();
    }
}