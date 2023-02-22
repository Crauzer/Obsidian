using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Obsidian.Data;
using Obsidian.Services;
using Photino.Blazor;
using PhotinoAPI;

namespace Obsidian;

public class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        PhotinoBlazorAppBuilder builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.Services.AddLogging();

        // register root component and selector
        builder.RootComponents.Add<App>("app");

        builder.Services.AddSingleton(Config.Load());
        builder.Services.AddScoped<HashtableService>();
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PreventDuplicates = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 5000;
            config.SnackbarConfiguration.ShowTransitionDuration = 250;
            config.SnackbarConfiguration.HideTransitionDuration = 250;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
        });

        PhotinoBlazorApp app = builder.Build();

        // customize window
        app.MainWindow.UseOsDefaultSize = false;
        app.MainWindow
            .SetIconFile("favicon.ico")
            .SetTitle("Obsidian")
            .Center()
            .SetWidth(1600)
            .SetHeight(1100)
            .SetContextMenuEnabled(false)
            .RegisterApi(new());

#if DEBUG
        app.MainWindow.SetDevToolsEnabled(true);
#endif
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.OpenAlertWindow("Fatal exception", error.ExceptionObject.ToString());
        };

        app.Run();
    }
}
