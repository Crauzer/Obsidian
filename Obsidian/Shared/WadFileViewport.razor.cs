using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta.Classes;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using Obsidian.Data.Wad;
using Obsidian.Utils;
using PhotinoNET;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Obsidian.Shared;

public partial class WadFileViewport
{
    [Inject]
    public PhotinoWindow Window { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Parameter]
    public WadTabModel WadTab { get; set; }

    private bool _isEnabled => this.WadTab.CurrentPreviewType is WadFilePreviewType.Viewport;

    private bool _isSavingAsGltf;

    public void SaveAsGltf()
    {
        CommonSaveFileDialog dialog =
            new() { DefaultFileName = Path.ChangeExtension(this.WadTab.SelectedFile.Name, "glb") };
        foreach (CommonFileDialogFilter filter in FileDialogUtils.CreateGltfFilters())
            dialog.Filters.Add(filter);

        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        ToggleIsSavingAsGltf(true);
        try
        {
            using Stream fileStream = this.WadTab.Wad.LoadChunkDecompressed(this.WadTab.SelectedFile.Chunk).AsStream();

            LeagueFileType fileType = LeagueFile.GetFileType(fileStream);
            ModelRoot gltf = fileType switch
            {
                LeagueFileType.SimpleSkin => CreateGltfFromSkinnedMesh(fileStream),
                _ => throw new InvalidOperationException($"Cannot save fileType: {fileType} as glTF")
            };

            gltf.Save(dialog.FileName);

            this.Snackbar.Add($"Saved {this.WadTab.SelectedFile.Name} as glTF!", Severity.Success);
        }
        catch (Exception exception)
        {
            SnackbarUtils.ShowHardError(this.Snackbar, exception);
        }
        finally
        {
            ToggleIsSavingAsGltf(false);
        }
    }

    private ModelRoot CreateGltfFromSkinnedMesh(Stream stream)
    {
        using SkinnedMesh skinnedMesh = SkinnedMesh.ReadFromSimpleSkin(stream);

        return skinnedMesh.ToGltf(new List<(string, Stream)>());
    }

    private void ToggleIsSavingAsGltf(bool value)
    {
        this._isSavingAsGltf = value;
        StateHasChanged();
    }
}
