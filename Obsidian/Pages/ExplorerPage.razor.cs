using LeagueToolkit.Core.Wad;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using Obsidian.Data.Wad;
using Obsidian.Services;
using Obsidian.Utils;
using PhotinoNET;
using XXHash3NET;

namespace Obsidian.Pages;

public partial class ExplorerPage
{
    [Inject]
    public HashtableService Hashtable { get; set; }

    [Inject]
    public PhotinoWindow Window { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    public List<WadTabModel> Tabs { get; set; } = new();
    public WadTabModel ActiveTab => this.Tabs.ElementAtOrDefault(this._activeTabId);
    private int _activeTabId = 0;

    private bool _isLoadingWadFile = false;
    private bool _isExportingFiles = false;

    public async Task OpenWad()
    {
        CommonOpenFileDialog dialog = new("Open Wad archives") { Multiselect = true };
        dialog.Filters.Add(FileDialogUtils.CreateWadFilter());

        if (dialog.ShowDialog(this.Window.WindowHandle) is CommonFileDialogResult.Cancel)
            return;

        this._isLoadingWadFile = true;
        StateHasChanged();
        try
        {
            await Task.Run(() => OpenWadFiles(dialog.FileNames));
            this.Snackbar.Add("Successfully opened Wad archives!", Severity.Success);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            this._isLoadingWadFile = false;
            StateHasChanged();
        }
    }

    public async Task ExtractAll()
    {
        string extractionDirectory = AskForExtractionDirectory();
        if (string.IsNullOrEmpty(extractionDirectory))
            return;

        IEnumerable<WadFileModel> fileItems = this.ActiveTab
            .TraverseFlattenedItems()
            .Where(x => x is WadFileModel)
            .Select(x => x as WadFileModel);

        ToggleExporting(true);
        try
        {
            await Task.Run(() => ExtractFiles(fileItems, extractionDirectory));

            this.Snackbar.Add(
                $"Successfully exported {fileItems.Count()} files!",
                Severity.Success
            );
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            ToggleExporting(false);
        }
    }

    public async Task ExtractSelected()
    {
        string extractionDirectory = AskForExtractionDirectory();
        if (string.IsNullOrEmpty(extractionDirectory))
            return;

        IEnumerable<WadFileModel> fileItems = this.ActiveTab.SelectedFiles;

        ToggleExporting(true);
        try
        {
            await Task.Run(() => ExtractFiles(fileItems, extractionDirectory));

            this.Snackbar.Add(
                $"Successfully exported {fileItems.Count()} files!",
                Severity.Success
            );
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            ToggleExporting(false);
        }
    }

    private void ExtractFiles(IEnumerable<WadFileModel> fileItems, string extractionDirectory)
    {
        WadFile wad = this.ActiveTab.Wad;
        foreach (WadFileModel fileItem in fileItems)
            Utils.WadUtils.SaveChunk(wad, fileItem.Chunk, fileItem.Path, extractionDirectory);
    }

    private void OpenWadFiles(IEnumerable<string> wadPaths)
    {
        foreach (string wadPath in wadPaths)
        {
            FileStream wadFileStream = File.OpenRead(wadPath);
            WadFile wad = new(wadFileStream);

            this.Tabs.Add(new(Path.GetFileName(wadPath), wad, this.Hashtable));
        }
    }

    private void RemoveWadTab(MudTabPanel tabPanel)
    {
        if (tabPanel.Tag is not Guid tabId)
            return;

        WadTabModel tab = this.Tabs.FirstOrDefault(x => x.Id == tabId);
        if (tab is not null)
        {
            tab.Wad.Dispose();
            this.Tabs.Remove(tab);
        }
    }

    private string AskForExtractionDirectory()
    {
        CommonOpenFileDialog dialog =
            new("Select the extraction directory") { IsFolderPicker = true };

        dialog.ShowDialog(this.Window.WindowHandle);

        return dialog.FileName;
    }

    public List<WadItemModel> GetVisibleItemsForActiveTab()
    {
        try
        {
            return this.ActiveTab?.GetFlattenedItems() ?? new();
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowSoftError(this.Snackbar, exception);

            return new();
        }
    }

    public void ToggleActiveTabRegexFilter()
    {
        this.ActiveTab.UseRegexFilter = !this.ActiveTab.UseRegexFilter;
        StateHasChanged();
    }

    public void ToggleExporting(bool isExporting)
    {
        this._isExportingFiles = isExporting;
        StateHasChanged();
    }

    public void RefreshState() => StateHasChanged();
}
