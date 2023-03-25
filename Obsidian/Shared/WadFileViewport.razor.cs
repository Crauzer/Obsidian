﻿using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Hashing;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta;
using LeagueToolkit.Meta.Classes;
using LeagueToolkit.Toolkit.Gltf;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using Obsidian.Data.Wad;
using Obsidian.Utils;
using PhotinoNET;
using Serilog;
using SharpGLTF.Schema2;
using RigResource = LeagueToolkit.Core.Animation.RigResource;
using Texture = LeagueToolkit.Core.Renderer.Texture;

namespace Obsidian.Shared;

public partial class WadFileViewport
{
    [Inject]
    public PhotinoWindow Window { get; set; }

    [Inject]
    public ISnackbar Snackbar { get; set; }

    [Parameter]
    public WadTreeModel WadTree { get; set; }

    private bool _isEnabled => this.WadTree.CurrentPreviewType is WadFilePreviewType.Viewport;

    private bool _isSavingAsGltf;

    private static readonly string[] DIFFUSE_SAMPLERS = new[]
    {
        "DiffuseTexture",
        "Diffuse_Texture"
    };

    public void SaveAsGltf()
    {
        CommonSaveFileDialog dialog =
            new() { DefaultFileName = Path.ChangeExtension(this.WadTree.SelectedFile.Name, "glb") };
        foreach (CommonFileDialogFilter filter in FileDialogUtils.CreateGltfFilters())
            dialog.Filters.Add(filter);

        if (dialog.ShowDialog(this.Window.WindowHandle) is not CommonFileDialogResult.Ok)
            return;

        Log.Information($"Saving {this.WadTree.SelectedFile.Path} as glTF to {dialog.FileName}");
        ToggleIsSavingAsGltf(true);
        try
        {
            using Stream fileStream = this.WadTree.SelectedFile.Wad
                .LoadChunkDecompressed(this.WadTree.SelectedFile.Chunk)
                .AsStream();

            LeagueFileType fileType = LeagueFile.GetFileType(fileStream);

            ModelRoot gltf = fileType switch
            {
                LeagueFileType.PropertyBin
                    when BinUtils.IsSkinPackage(this.WadTree.SelectedFile.Path)
                    => CreateGltfFromSkinPackage(this.WadTree.SelectedFile.Wad, fileStream),
                LeagueFileType.StaticMeshAscii => StaticMesh.ReadAscii(fileStream).ToGltf(),
                LeagueFileType.StaticMeshBinary => StaticMesh.ReadBinary(fileStream).ToGltf(),
                _
                    => throw new InvalidOperationException(
                        $"Cannot save fileType: {fileType} as glTF"
                    )
            };

            gltf.Save(dialog.FileName);

            this.Snackbar.Add($"Saved {this.WadTree.SelectedFile.Name} as glTF!", Severity.Success);
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

    private ModelRoot CreateGltfFromSkinPackage(WadFile wad, Stream stream)
    {
        BinTree skinPackage = new(stream);
        MetaEnvironment metaEnvironment = BinUtils.CreateMetaEnvironment();

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

        using Stream simpleSkinStream = wad.LoadChunkDecompressed(meshData.SimpleSkin).AsStream();
        using Stream skeletonStream = wad.LoadChunkDecompressed(meshData.Skeleton).AsStream();

        using SkinnedMesh skinnedMesh = SkinnedMesh.ReadFromSimpleSkin(simpleSkinStream);
        RigResource skeleton = new(skeletonStream);

        return skinnedMesh.ToGltf(
            skeleton,
            SkinnedMeshUtils.CollectMaterialTextures(
                skinnedMesh,
                meshData,
                skinPackage,
                wad,
                metaEnvironment
            ),
            SkinnedMeshUtils.LoadAnimationAssets(skinData, skinPackage, wad, metaEnvironment)
        );
    }

    private void ToggleIsSavingAsGltf(bool value)
    {
        this._isSavingAsGltf = value;
        StateHasChanged();
    }
}
