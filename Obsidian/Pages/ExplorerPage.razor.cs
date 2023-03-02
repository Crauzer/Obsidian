using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Renderer;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Hashing;
using LeagueToolkit.Meta;
using LeagueToolkit.Meta.Classes;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using MudExtensions;
using Obsidian.BabylonJs;
using Obsidian.Data;
using Obsidian.Data.Wad;
using Obsidian.Services;
using Obsidian.Shared;
using Obsidian.Utils;
using PhotinoNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Toolbelt.Blazor.HotKeys2;
using RigResource = LeagueToolkit.Core.Animation.RigResource;

namespace Obsidian.Pages;

public partial class ExplorerPage : IDisposable
{
  #region Injection
    [Inject]
    public Config Config { get; set; }

    [Inject]
    public HashtableService Hashtable { get; set; }

    [Inject]
    public PhotinoWindow Window { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Inject]
    public HotKeys HotKeys { get; set; }

    [Inject]
    public IJSRuntime JsRuntime { get; set; }
  #endregion

    public HotKeysContext _hotKeysContext;

    public WadFilter WadFilterComponent { get; set; }

    public List<WadTabModel> Tabs { get; set; } = new();

    public WadTabModel ActiveTab => this.Tabs.ElementAtOrDefault(this.ActiveTabId);
    public int ActiveTabId { get; set; }

    private MudTabs _tabsComponent;

    private MudSplitter _splitter;

    private bool _isLoadingWadFile = false;
    private bool _isExportingFiles = false;
    private bool _isLoadingHashtable = false;

    public async Task OpenWad()
    {
        CommonOpenFileDialog dialog = FileDialogUtils.CreateOpenWadDialog(
            this.Config.GameDataDirectory
        );
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
        CommonOpenFileDialog dialog = FileDialogUtils.CreateExtractWadDialog(
            this.Config.DefaultExtractDirectory
        );
        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        IEnumerable<WadFileModel> fileItems = this.ActiveTab
            .TraverseFlattenedItems()
            .Where(x => x is WadFileModel)
            .Select(x => x as WadFileModel);

        ToggleExporting(true);
        try
        {
            await Task.Run(() => ExtractFiles(fileItems, dialog.FileName));

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
        CommonOpenFileDialog dialog = FileDialogUtils.CreateExtractWadDialog(
            this.Config.DefaultExtractDirectory
        );
        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        IEnumerable<WadFileModel> fileItems = this.ActiveTab.CheckedFiles;

        ToggleExporting(true);
        try
        {
            await Task.Run(() => ExtractFiles(fileItems, dialog.FileName));

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

        // Set opened wad as active
        this.ActiveTabId = this.Tabs.Count - 1;
    }

    private async Task RemoveWadTab(MudTabPanel tabPanel)
    {
        if (tabPanel.Tag is not Guid tabId)
            return;

        WadTabModel tab = this.Tabs.FirstOrDefault(x => x.Id == tabId);
        if (tab is not null)
        {
            await this.JsRuntime.InvokeVoidAsync(
                "destroyThreeJsRenderer",
                tab.GetViewportContainerId()
            );

            tab.Wad.Dispose();
            this.Tabs.Remove(tab);
        }
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

    // TODO: Refactor this to be an event from the tree view itself
    public async Task UpdateSelectedFile()
    {
        // Hide the preview if the selected file is null
        if (this.ActiveTab.SelectedFile is null)
        {
            await SetCurrentPreviewType(WadFilePreviewType.None);
            return;
        }

        try
        {
            await PreviewSelectedFile(this.ActiveTab.SelectedFile);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowSoftError(this.Snackbar, exception);
        }
    }

    private async Task PreviewSelectedFile(WadFileModel file)
    {
        using Stream fileStream = this.ActiveTab.Wad.LoadChunkDecompressed(file.Chunk).AsStream();
        LeagueFileType fileType = LeagueFile.GetFileType(fileStream);

        if (BinUtils.IsSkinPackage(file.Path))
        {
            await PreviewSkinPackage(fileStream);
        }
        else if (fileType is (LeagueFileType.TextureDds or LeagueFileType.Texture))
        {
            await PreviewImage(ImageUtils.GetImageFromStream(fileStream));
        }
        else
        {
            await SetCurrentPreviewType(WadFilePreviewType.None);
        }
    }

    private async Task PreviewSkinPackage(Stream stream)
    {
        BinTree skinPackage = new(stream);
        var metaEnvironment = MetaEnvironment.Create(
            Assembly.Load("LeagueToolkit.Meta.Classes").ExportedTypes.Where(x => x.IsClass)
        );

        BinTreeObject skinDataObject = skinPackage.Objects.Values.FirstOrDefault(
            x => x.ClassHash == Fnv1a.HashLower(nameof(SkinCharacterDataProperties))
        );

        if (skinDataObject is null)
            throw new InvalidDataException(
                $"Skin package does not contain {nameof(SkinCharacterDataProperties)}"
            );

        var skinData = MetaSerializer.Deserialize<SkinCharacterDataProperties>(
            metaEnvironment,
            skinDataObject
        );
        SkinMeshDataProperties meshData = skinData.SkinMeshProperties;

        using Stream simpleSkinStream = this.ActiveTab.Wad
            .LoadChunkDecompressed(meshData.SimpleSkin)
            .AsStream();
        using Stream skeletonStream = this.ActiveTab.Wad
            .LoadChunkDecompressed(meshData.Skeleton)
            .AsStream();

        using SkinnedMesh skinnedMesh = SkinnedMesh.ReadFromSimpleSkin(simpleSkinStream);
        RigResource skeleton = new(skeletonStream);

        // Make sure viewport is created
        await SetCurrentPreviewType(WadFilePreviewType.Viewport);
        await Task.Delay(50);

        await Three.CreateSkinnedMesh(
            this.JsRuntime,
            this.ActiveTab.GetViewportContainerId(),
            skinnedMesh,
            skeleton,
            await SkinnedMeshUtils.CreateTextureImages(
                this.JsRuntime,
                skinnedMesh,
                meshData,
                skinPackage,
                this.ActiveTab.Wad,
                metaEnvironment
            )
        );
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

        await SetCurrentPreviewType(WadFilePreviewType.Image);
    }

    private async Task SetCurrentPreviewType(WadFilePreviewType previewType)
    {
        if (this.ActiveTab.CurrentPreviewType is WadFilePreviewType.Viewport)
            await this.JsRuntime.InvokeVoidAsync(
                "destroyThreeJsRenderer",
                this.ActiveTab.GetViewportContainerId()
            );

        this.ActiveTab.CurrentPreviewType = previewType;
        StateHasChanged();
    }

    private async Task OnDimensionChanged(double dimension)
    {
        if (
            this.ActiveTab is not null
            && this.ActiveTab.CurrentPreviewType is WadFilePreviewType.Viewport
        )
            await this.JsRuntime.InvokeVoidAsync(
                "resizeViewport",
                this.ActiveTab.GetViewportContainerId()
            );
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

  #region Hotkey Handlers
    private async ValueTask FocusWadFilter()
    {
        await this.WadFilterComponent.InputField.FocusAsync();
    }
  #endregion

    protected override void OnInitialized()
    {
        this._hotKeysContext = this.HotKeys
            .CreateContext()
            .Add(ModCode.Ctrl, Code.F, FocusWadFilter, "Focus Wad Filter");
    }

    public void Dispose()
    {
        this._hotKeysContext?.Dispose();
    }
}
