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
    public WadTabModel ActiveTab => this.Tabs.ElementAt(this._activeTabId);

    private bool _isLoadingWadFile = false;
    private bool _isExportingFiles = false;

    private int _filesExportProgress = 0;
    private int _filesExportCount = 0;

    private int _activeTabId = 0;

    private delegate Task ReportExtractionProgressDelegate(int progress);

    public async Task OpenWad()
    {
        CommonOpenFileDialog dialog = new("Open Wad archives") { Multiselect = true };
        dialog.Filters.Add(FileDialogUtils.CreateWadFilter());

        if (dialog.ShowDialog(this.Window.WindowHandle) is CommonFileDialogResult.Cancel)
            return;

        this._isLoadingWadFile = true;
        try
        {
            await Task.Run(() => OpenWadFiles(dialog.FileNames));
            this.Snackbar.Add("Successfully opened Wad archives!", Severity.Success);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowError(this.Snackbar, exception);
        }
        finally
        {
            this._isLoadingWadFile = false;
        }
    }

    public async Task ExtractAll()
    {
        if (this.Tabs.Count == 0)
        {
            this.Snackbar.Add("You need to open a Wad archive!", Severity.Error);
            return;
        }

        string extractionDirectory = AskForExtractionDirectory();
        if (string.IsNullOrEmpty(extractionDirectory))
            return;

        IEnumerable<WadFileModel> allFileItems = this.ActiveTab
            .TraverseFlattenedItems()
            .Where(x => x is WadFileModel)
            .Select(x => x as WadFileModel);

        this._isExportingFiles = true;
        this._filesExportCount = allFileItems.Count();
        StateHasChanged();
        try
        {
            await ExtractFiles(allFileItems, extractionDirectory, ReportFileExtractionProgress);

            this.Snackbar.Add(
                $"Successfully exported {this._filesExportCount} files!",
                Severity.Success
            );
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowError(this.Snackbar, exception);
        }
        finally
        {
            this._isExportingFiles = false;
            this._filesExportProgress = 0;
            StateHasChanged();
        }
    }

    public async Task ExtractSelected()
    {
        if (this.Tabs.Count == 0)
        {
            this.Snackbar.Add("You need to open a Wad archive!", Severity.Error);
            return;
        }
    }

    private async Task ExtractFiles(
        IEnumerable<WadFileModel> fileItems,
        string extractionDirectory,
        ReportExtractionProgressDelegate reportProgress
    )
    {
        int currentFileId = 0;
        WadFile wad = this.ActiveTab.Wad;
        foreach (WadFileModel fileItem in fileItems)
        {
            string filePath = CreateWadChunkFilePath(extractionDirectory, fileItem.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using FileStream chunkFileStream = File.Create(filePath);
            using Stream chunkStream = wad.OpenChunk(fileItem.Chunk);

            chunkStream.CopyTo(chunkFileStream);

            await reportProgress(currentFileId++);
        }
    }

    private string CreateWadChunkFilePath(string extractionDirectory, string chunkPath)
    {
        string naivePath = Path.Join(extractionDirectory, chunkPath);
        if (naivePath.Length <= 260)
            return naivePath;

        return Path.Join(
            extractionDirectory,
            string.Format(
                "{0:x16}.{1}",
                XXHash64.Compute(chunkPath.ToLower()),
                Path.GetExtension(chunkPath)
            )
        );
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
        if (tabPanel.ID is not Guid tabId)
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

    private async Task ReportFileExtractionProgress(int progress) =>
        await InvokeAsync(() =>
        {
            this._filesExportProgress = progress;
            StateHasChanged();
        });

    public void RefreshState() => StateHasChanged();
}
