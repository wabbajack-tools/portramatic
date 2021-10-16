using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Portramatic;

public class TempFile : IDisposable
{
    private readonly string _name;

    public string Name => _name;

    public TempFile(string extension = "")
    {
        _name = Path.GetTempFileName() + extension;
    }
    
    public void Dispose()
    {
        if (File.Exists(_name))
            File.Delete(_name);
    }

    public static async Task<TempFile> Resource(string path)
    {
        var file = new TempFile();
        await using var stream = typeof(TempFile).Assembly.GetManifestResourceStream(path);
        await using var os = File.Open(file.Name, FileMode.Create);
        await stream!.CopyToAsync(os);
        return file;
    }
}