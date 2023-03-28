using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Obsidian.Data;
using Obsidian.Data.Wad;
using Obsidian.Services;
using Obsidian.Utils;
using Toolbelt.Blazor.HotKeys2;

namespace Obsidian.Pages;

public partial class ExplorerPage
{
    #region Injection
    [Inject]
    public Config Config { get; set; }

    [Inject]
    public HashtableService Hashtable { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Inject]
    public HotKeys HotKeys { get; set; }

    [Inject]
    public IJSRuntime JsRuntime { get; set; }
    #endregion

    public WadTreeModel WadTree { get; set; }

    protected override void OnInitialized()
    {
        _ = InvokeAsync(async () =>
        {
            try
            {
                await Task.Run(() =>
                {
                    this.WadTree = string.IsNullOrEmpty(this.Config.GameDataDirectory) switch
                    {
                        true => new(this.Hashtable, this.Config, Array.Empty<string>()),
                        false
                            => new(
                                this.Hashtable,
                                this.Config,
                                Directory.EnumerateFiles(
                                    this.Config.GameDataDirectory,
                                    "*.wad.client",
                                    SearchOption.AllDirectories
                                )
                            )
                    };
                });
                StateHasChanged();
            }
            catch (Exception exception)
            {
                SnackbarUtils.ShowHardError(this.Snackbar, exception);
            }
        });
    }
}
