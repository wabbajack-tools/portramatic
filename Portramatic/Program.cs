using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Wabbajack.Paths;

namespace Portramatic
{
    
    // To Publish: 
    // dotnet publish -r win-x64 -c Release -p:PublishReadyToRun=true --self-contained -o c:\tmp\publish -p:PublishSingleFile=true -p:DebugType=embedded -p:IncludeAllContentForSelfExtract=true
    // "C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64\signtool.exe" sign /t http://timestamp.sectigo.com c:\tmp\publish\Portramatic.exe
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Program.AdminPath = args[0].ToAbsolutePath();
            }
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AbsolutePath AdminPath { get; set; }
        public static bool IsAdminMode => AdminPath != default;

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
