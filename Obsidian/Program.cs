using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Obsidian.Data;
using Obsidian.Services;
using Photino.Blazor;
using PhotinoAPI;
using Semver;
using Serilog;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Obsidian;

public class Program
{
    public static readonly SemVersion VERSION = SemVersion.Parse("5.1.2", SemVersionStyles.Strict);

    [STAThread]
    static void Main(string[] args)
    {
        InitializeLogger();

        Log.Information($"Version: {VERSION}");
        Log.Information("Building app");

        PhotinoBlazorAppBuilder builder = PhotinoBlazorAppBuilder.CreateDefault(args);
        builder.Services.AddLogging();
        // register root component and selector
        builder.RootComponents.Add<App>("app");

        builder.Services.AddSingleton(Config.Load());
        builder.Services.AddSingleton<DiscordRichPresence>();
        builder.Services.AddScoped<HashtableService>();
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.ShowTransitionDuration = 250;
            config.SnackbarConfiguration.HideTransitionDuration = 250;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
        });
        builder.Services.AddMudExtensions();
        builder.Services.AddHotKeys2();

        PhotinoBlazorApp app = builder.Build();

        Log.Information("Customizing window");

        // customize window
        app.MainWindow.UseOsDefaultSize = false;
        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("Obsidian")
            .SetUseOsDefaultSize(true)
            .SetContextMenuEnabled(false)
            .RegisterApi(new());

        app.MainWindow.SetDevToolsEnabled(true);

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.OpenAlertWindow("Fatal exception", error.ExceptionObject.ToString());

            Log.Fatal($"Fatal error: {error.ExceptionObject}");
        };

        Log.Information("Running app");
        app.Run();
    }

    private static void InitializeLogger()
    {
        Directory.CreateDirectory("logs");

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        Log.Logger = new LoggerConfiguration().WriteTo
            .File(
                $"logs/{timestamp}_obsidianlog.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
    }
}
