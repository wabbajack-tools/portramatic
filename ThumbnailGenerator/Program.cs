// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Portramatic.DTOs;
using Portramatic.ViewModels;
using ReactiveUI;
using Shipwreck.Phash;
using SkiaSharp;
using ThumbnailGenerator;

class Program
{
    private static HttpClient Client = new();

    public static async Task<int> Main(string[] args)
    {
        var files = Directory.EnumerateFiles(Path.Combine(args[0], "Definitions"), "definition.json",
                SearchOption.AllDirectories)
            .ToArray();

        Console.WriteLine($"Found {files.Length} definitions, loading...");

        var definitions = new List<(PortraitDefinition, string)>();

        foreach (var file in files)
        {
            definitions.Add((JsonSerializer.Deserialize<PortraitDefinition>(await File.ReadAllTextAsync(file),
                new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                })!, file));
        }

        Console.WriteLine($"Loaded {definitions.Count} definitions, creating gallery files");

        var outputMemoryStream = new MemoryStream();
        {
            using var archive = new ZipArchive(outputMemoryStream, ZipArchiveMode.Update, true);
            {
                var badLinks = new ConcurrentBag<string>();
                var hashes =
                    new ConcurrentBag<(PortraitDefinition Definition, Digest Hash, Size Size, string Filename)>();
                
                await Parallel.ForEachAsync(definitions.Select((v, idx) => (v.Item1, v.Item2,  idx)), async (itm, token) =>
                {
                    var (definition, path, idx) = itm;
                    Console.WriteLine(
                        $"[{idx}/{definitions.Count}]Adding {definition.Source.ToString().Substring(0, Math.Min(70, definition.Source.ToString().Length))}");
                    try
                    {
                        var (hash, size) = await GenerateThumbnail(archive, definition);
                        hashes.Add((definition, hash, size, path));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"BAD: {definition.MD5}");
                        //File.Delete(path);
                        badLinks.Add(definition.MD5);
                    }
                });
                
                Console.WriteLine("Finding duplicate images");

                var grouped = from src in hashes.AsParallel()
                    from dest in hashes
                    where src.Definition.MD5 != dest.Definition.MD5
                    let cross = ImagePhash.GetCrossCorrelation(src.Hash, dest.Hash)
                    where cross >= 0.99f
                    where src.Size.Width * src.Size.Height >= dest.Size.Width * dest.Size.Height
                    group (dest, cross) by src.Definition.MD5 into result
                    select result;
                
                var results = grouped.ToArray();
                
                Console.WriteLine($"Found {results.Length} duplicate pairs");
                foreach (var result in results)
                {
                    foreach (var (dest, _) in result)
                    {
                        Console.WriteLine($"Removing {dest.Definition.MD5}");
                        badLinks.Add(dest.Definition.MD5);
                        if (File.Exists(dest.Filename))
                            File.Delete(dest.Filename);
                        archive.GetEntry(dest.Definition.MD5+".webp")?.Delete();
                    }
                }
                
                definitions = definitions.Where(d => !badLinks.Contains(d.Item1.MD5)).ToList();
                {
                    var tocEntry = archive.CreateEntry("definitions.json", CompressionLevel.SmallestSize);
                    await using var tocStream = tocEntry.Open();
                    await using var sw = new StreamWriter(tocStream);
                    var json = JsonSerializer.Serialize(definitions.Select(d => d.Item1).ToArray(), new JsonSerializerOptions()
                    {
                        WriteIndented = false,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    });
                    await sw.WriteAsync(json);
                }
            }
        }
        Console.WriteLine($"Output zip is {outputMemoryStream.Length} bytes in size");
        await File.WriteAllBytesAsync(Path.Combine(args[1], "gallery.zip"), outputMemoryStream.ToArray());
        return 0;
    }

    private static async Task<(Digest Hash, Size)> GenerateThumbnail(ZipArchive archive, PortraitDefinition definition)
    {
        var bitmapBytes = await Client.GetByteArrayAsync(definition.Source);
        var (width, height) = definition.Full.FinalSize;
        using var src = SKImage.FromEncodedData(new MemoryStream(bitmapBytes));

        
        var ibitmap = new SKImageIBitmap(src);
        var hash = ImagePhash.ComputeDigest(ibitmap);

        var cropped = definition.Crop(src, ImageSize.Full);
        var snap = Resize(cropped, width / 4, height / 4);

        var encoded = snap.Encode(SKEncodedImageFormat.Webp, 50);

        if (string.IsNullOrEmpty(definition.MD5))
            throw new InvalidDataException("MD5");

        lock (archive)
        {
            var entry = archive.CreateEntry(definition.MD5 + ".webp");
            using var entryStream = entry.Open();
            encoded.SaveTo(entryStream);
        }

        return (hash, new Size(src.Width, src.Height));
    }

    private static SKImage Resize(SKImage i, int width, int height)
    {
        var paint = new SKPaint();
        paint.FilterQuality = SKFilterQuality.High;
        
        using var surface =
            SKSurface.Create(new SKImageInfo(i.Width / 4, i.Height / 4, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.DrawImage(i, new SKRect(0, 0, i.Width, i.Height),
            new SKRect(0, 0, (float)i.Width / 4, (float)i.Height / 4),
            paint);
        return surface.Snapshot();
    }
}