using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Memory;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Renderer;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Toolkit;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using MudExtensions;
using Obsidian.Data.Wad;
using Obsidian.Services;
using Obsidian.Utils;
using PhotinoNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
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

    private MudSplitter _splitter;

    private bool _isLoadingWadFile = false;
    private bool _isExportingFiles = false;
    private bool _isLoadingHashtable = false;

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

        IEnumerable<WadFileModel> fileItems = this.ActiveTab.CheckedFiles;

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

    public async Task LoadHashtable()
    {
        CommonOpenFileDialog dialog = new("Select hashtables") { Multiselect = true };
        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        ToggleLoadingHashtable(true);
        try
        {
            await Task.Run(() =>
            {
                foreach (string hashtableFile in dialog.FileNames)
                {
                    this.Hashtable.LoadHashtable(hashtableFile);
                }
            });

            this.Snackbar.Add("Successfully loaded hashtables!", Severity.Success);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            ToggleLoadingHashtable(false);
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

    // TODO: Refactor this to be an event from the tree view itself
    public async Task UpdateSelectedFile()
    {
        // Hide the preview if the selected file is null
        if (this.ActiveTab.SelectedFile is null)
        {
            SetCurrentPreviewType(WadFilePreviewType.None);
            return;
        }

        try
        {
            using Stream fileStream = this.ActiveTab.Wad.LoadChunkDecompressed(this.ActiveTab.SelectedFile.Chunk).AsStream();

            await PreviewSelectedFile(fileStream);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowSoftError(this.Snackbar, exception);
        }
    }

    private async Task PreviewSelectedFile(Stream fileStream)
    {
        LeagueFileType fileType = LeagueFile.GetFileType(fileStream);
        if (fileType is (LeagueFileType.TextureDds or LeagueFileType.Texture))
        {
            await PreviewImage(ImageUtils.GetImageFromStream(fileStream));
            SetCurrentPreviewType(WadFilePreviewType.Image);
        }
        else if (fileType is LeagueFileType.SimpleSkin)
        {
            await PreviewSimpleSkin(fileStream);
            SetCurrentPreviewType(WadFilePreviewType.Viewport);

            // TODO: Move this into some generic function
            // Fixes viewport dpi issue
            await this.JsRuntime.InvokeVoidAsync("resizeBabylonEngine", this.ActiveTab.GetViewportCanvasId());
        }
        else
        {
            SetCurrentPreviewType(WadFilePreviewType.None);
        }
    }

    private async Task PreviewImage(Image<Rgba32> image)
    {
        MemoryStream imageStream = new();
        
        await image.SaveAsPngAsync(imageStream);
        imageStream.Position = 0;

        DotNetStreamReference jsStream = new(imageStream);

        await this.JsRuntime.InvokeVoidAsync(
            "setImage",
            $"{this.ActiveTab.Id}_imagePreview",
            jsStream
        );
    }

    private async Task PreviewSimpleSkin(Stream fileStream)
    {
        using SkinnedMesh skinnedMesh = SkinnedMesh.ReadFromSimpleSkin(fileStream);

        // Create vertex data for babylon
        float[] positions = CreateVector3Data(
            skinnedMesh.VerticesView.GetAccessor(ElementName.Position).AsVector3Array()
        );
        float[] normals = CreateVector3Data(
            skinnedMesh.VerticesView.GetAccessor(ElementName.Normal).AsVector3Array()
        );
        float[] uvs = CreateVector2Data(
            skinnedMesh.VerticesView.GetAccessor(ElementName.Texcoord0).AsVector2Array()
        );

        uint[] indices = skinnedMesh.Indices.ToArray();

        await this.JsRuntime.InvokeVoidAsync(
            "initBabylonCanvas",
            this.ActiveTab.GetViewportCanvasId()
        );
        await this.JsRuntime.InvokeVoidAsync(
            "renderSkinnedMesh",
            this.ActiveTab.GetViewportCanvasId(),
            skinnedMesh.Ranges,
            indices,
            positions,
            normals,
            uvs
        );

        static float[] CreateVector2Data(IReadOnlyList<Vector2> array)
        {
            int dataOffset = 0;
            var data = new float[array.Count * 2];
            for (int i = 0; i < array.Count; i++)
            {
                data[dataOffset + 0] = array[i].X;
                data[dataOffset + 1] = array[i].Y;

                dataOffset += 2;
            }

            return data;
        }
        static float[] CreateVector3Data(IReadOnlyList<Vector3> array)
        {
            int dataOffset = 0;
            var data = new float[array.Count * 3];
            for (int i = 0; i < array.Count; i++)
            {
                data[dataOffset + 0] = array[i].X;
                data[dataOffset + 1] = array[i].Y;
                data[dataOffset + 2] = array[i].Z;

                dataOffset += 3;
            }

            return data;
        }
    }

    private void SetCurrentPreviewType(WadFilePreviewType previewType)
    {
        this.ActiveTab.CurrentPreviewType = previewType;
        StateHasChanged();
    }

    private async Task OnDimensionChanged(double dimension)
    {
        if(this.ActiveTab is not null && this.ActiveTab.CurrentPreviewType is WadFilePreviewType.Viewport)
            await this.JsRuntime.InvokeVoidAsync("resizeBabylonEngine", this.ActiveTab.GetViewportCanvasId());
    }

    public void ToggleExporting(bool isExporting)
    {
        this._isExportingFiles = isExporting;
        StateHasChanged();
    }

    public void ToggleLoadingHashtable(bool value)
    {
        this._isLoadingHashtable = value;
        StateHasChanged();
    }

    public void RefreshState() => StateHasChanged();
}
