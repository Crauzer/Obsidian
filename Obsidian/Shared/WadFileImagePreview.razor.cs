using CommunityToolkit.HighPerformance;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using Obsidian.Data.Wad;
using Obsidian.Utils;
using PhotinoNET;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Obsidian.Shared;

public partial class WadFileImagePreview
{
    [Inject]
    public PhotinoWindow Window { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Parameter]
    public bool Hidden { get; set; }

    [Parameter]
    public WadTreeModel WadTree { get; set; }

    [Parameter]
    public EventCallback OnSavingAsPng { get; set; }

    private bool _isSavingAsPng;

    private void SaveAsPng()
    {
        CommonSaveFileDialog dialog = new() { DefaultFileName = this.WadTree.SelectedFile.Name };
        dialog.Filters.Add(new("Image", "png"));

        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        Log.Information($"Saving {this.WadTree.SelectedFile.Path} as PNG to {dialog.FileName}");
        ToggleIsSavingAsPng(true);
        try
        {
            using Stream fileStream = this.WadTree.SelectedFile.Wad
                .LoadChunkDecompressed(this.WadTree.SelectedFile.Chunk)
                .AsStream();
            LeagueFileType fileType = LeagueFile.GetFileType(fileStream);

            Image<Rgba32> image = fileType switch
            {
                LeagueFileType.Texture
                or LeagueFileType.TextureDds
                    => ImageUtils.GetImageFromTextureStream(fileStream),
                LeagueFileType.Png or LeagueFileType.Jpeg => Image.Load<Rgba32>(fileStream),
                _
                    => throw new InvalidDataException(
                        $"Failed to create Image for fileType: {fileType}"
                    )
            };

            image.SaveAsPng(dialog.FileName);

            this.Snackbar.Add($"Saved {this.WadTree.SelectedFile.Name} as PNG!", Severity.Success);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            ToggleIsSavingAsPng(false);
        }
    }

    private void ToggleIsSavingAsPng(bool value)
    {
        this._isSavingAsPng = value;
        StateHasChanged();
    }
}
