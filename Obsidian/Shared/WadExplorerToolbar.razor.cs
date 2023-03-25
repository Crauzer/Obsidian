using Microsoft.AspNetCore.Components;
using Obsidian.Data.Wad;
using Obsidian.Pages;
using Toolbelt.Blazor.HotKeys2;

namespace Obsidian.Shared;

public partial class WadExplorerToolbar : IDisposable
{
    [Inject]
    public HotKeys HotKeys { get; set; }

    [CascadingParameter]
    public ExplorerPage ExplorerPage { get; set; }

    [Parameter]
    public WadTreeModel WadTree { get; set; }

    [Parameter]
    public EventCallback OnOpenWad { get; set; }

    [Parameter]
    public EventCallback OnExtractAll { get; set; }

    [Parameter]
    public EventCallback OnExtractSelected { get; set; }

    [Parameter]
    public EventCallback OnLoadHashtable { get; set; }

    private HotKeysContext _hotKeysContext;

    private WadFilter _wadFilterComponent;

    public AppTheme Theme { get; } = new();

    private void OnFilterChanged(string value)
    {
        this.WadTree.Filter = value;
        this.ExplorerPage.RefreshState();
    }

    private void OnUseRegexFilterChanged(bool value)
    {
        this.WadTree.UseRegexFilter = value;
        this.ExplorerPage.RefreshState();
    }

    private async ValueTask FocusWadFilter() =>
        await this._wadFilterComponent.InputField.FocusAsync();

    protected override void OnInitialized()
    {
        this._hotKeysContext = this.HotKeys
            .CreateContext()
            .Add(ModCode.Ctrl, Code.F, FocusWadFilter, "Focus Wad Filter")
            .Add(ModCode.Ctrl, Code.O, async () => await OnOpenWad.InvokeAsync(), "Open Wad");
    }

    public void Dispose()
    {
        this._hotKeysContext?.Dispose();
    }
}
