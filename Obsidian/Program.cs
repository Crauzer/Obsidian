﻿using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Obsidian.Data;
using Obsidian.Services;
using Photino.Blazor;
using PhotinoAPI;
using Semver;
using Serilog;
using System.Reflection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Obsidian;

public class Program {
    public static readonly SemVersion VERSION = SemVersion.FromVersion(
        Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyFileVersionAttribute)) switch {
            AssemblyFileVersionAttribute fileVersion => Version.Parse(fileVersion.Version),
            _ => typeof(Program).Assembly.GetName().Version
        }
    );

    [STAThread]
    static void Main(string[] args) {
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
        builder.Services.AddMudServices(config => {
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
            .SetTitle(string.Empty)
            .SetUseOsDefaultSize(true)
            .SetContextMenuEnabled(false)
            .RegisterWindowCreatedHandler(WindowCreatedHandler)
            .RegisterApi(new());

        app.MainWindow.SetDevToolsEnabled(true);

        AppDomain.CurrentDomain.UnhandledException += (sender, error) => {
            app.MainWindow.OpenAlertWindow("Fatal exception", error.ExceptionObject.ToString());

            Log.Fatal($"Fatal error: {error.ExceptionObject}");
        };

        Log.Information("Running app");
        app.Run();
    }

    private static void InitializeLogger() {
        Directory.CreateDirectory("logs");

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        Log.Logger = new LoggerConfiguration().WriteTo
            .File(
                $"logs/{timestamp}_obsidianlog.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
    }

    // used to workaround issues with the taskbar icon going missing, see also
    // https://github.com/tryphotino/photino.NET/issues/106 and
    // https://github.com/tryphotino/photino.NET/issues/85
    private static void WindowCreatedHandler(object sender, EventArgs e)
    {
        ((PhotinoNET.PhotinoWindow)sender).SetTitle("Obsidian");
    }
}