using CommunityToolkit.HighPerformance;
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
    [Inject] public PhotinoWindow Window { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }

    [Parameter]
    public WadTabModel WadTab { get; set; }

    [Parameter]
    public EventCallback OnSavingAsPng { get; set; }

    private bool _isEnabled => this.WadTab.CurrentPreviewType is WadFilePreviewType.Image;

    private bool _isSavingAsPng;

    private void SaveAsPng()
    {
        CommonSaveFileDialog dialog = new() { DefaultFileName = this.WadTab.SelectedFile.Name };
        dialog.Filters.Add(new("Image", "png"));

        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        Log.Information($"Saving {this.WadTab.SelectedFile.Path} as PNG to {dialog.FileName}");
        ToggleIsSavingAsPng(true);
        try
        {
            using Stream fileStream = this.WadTab.Wad.LoadChunkDecompressed(this.WadTab.SelectedFile.Chunk).AsStream();
            Image<Rgba32> image = ImageUtils.GetImageFromTextureStream(fileStream);

            image.SaveAsPng(dialog.FileName);

            this.Snackbar.Add($"Saved {this.WadTab.SelectedFile.Name} as PNG!", Severity.Success);
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
