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

    private WadTreeModel CreateWadTree()
    {
        return string.IsNullOrEmpty(this.Config.GameDataDirectory) switch
        {
            true => CreateEmptyWadTree(),
            false => TryCreateWadTree()
        };

        WadTreeModel TryCreateWadTree()
        {
            try
            {
                return new(
                    this.Hashtable,
                    this.Config,
                    Directory
                        .EnumerateFiles(
                            this.Config.GameDataDirectory,
                            "*.*",
                            SearchOption.AllDirectories
                        )
                        .Where(x => x.EndsWith(".wad") || x.EndsWith(".wad.client"))
                );
            }
            catch (Exception exception)
            {
                SnackbarUtils.ShowHardError(
                    this.Snackbar,
                    new InvalidOperationException("Failed to create wad tree", exception)
                );

                return CreateEmptyWadTree();
            }
        }
    }

    private WadTreeModel CreateEmptyWadTree() =>
        new(this.Hashtable, this.Config, Array.Empty<string>());

    private async Task RebuildWadTree()
    {
        await InvokeAsync(() =>
        {
            this.WadTree.Dispose();
            this.WadTree = null;

            StateHasChanged();
        });

        await InvokeAsync(async () =>
        {
            await Task.Run(() =>
            {
                this.WadTree = CreateWadTree();
            });
            StateHasChanged();
        });
    }

    protected override void OnInitialized()
    {
        _ = InvokeAsync(async () =>
        {
            try
            {
                await Task.Run(() =>
                {
                    this.WadTree = CreateWadTree();
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
