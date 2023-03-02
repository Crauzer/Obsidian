using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Hashing;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta;
using LeagueToolkit.Meta.Classes;
using LeagueToolkit.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using Obsidian.Data.Wad;
using Obsidian.Utils;
using PhotinoNET;
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
    public WadTabModel WadTab { get; set; }

    private bool _isEnabled => this.WadTab.CurrentPreviewType is WadFilePreviewType.Viewport;

    private bool _isSavingAsGltf;

    private static readonly string[] DIFFUSE_SAMPLERS = new[]
    {
        "DiffuseTexture",
        "Diffuse_Texture"
    };

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
            using Stream fileStream = this.WadTab.Wad
                .LoadChunkDecompressed(this.WadTab.SelectedFile.Chunk)
                .AsStream();

            LeagueFileType fileType = LeagueFile.GetFileType(fileStream);
            if (BinUtils.IsSkinPackage(this.WadTab.SelectedFile.Path))
            {
                CreateGltfFromSkinPackage(fileStream).Save(dialog.FileName);
            }
            else
            {
                throw new InvalidOperationException($"Cannot save fileType: {fileType} as glTF");
            }

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

    private ModelRoot CreateGltfFromSkinPackage(Stream stream)
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

        using Stream simpleSkinStream = this.WadTab.Wad
            .LoadChunkDecompressed(meshData.SimpleSkin)
            .AsStream();
        using Stream skeletonStream = this.WadTab.Wad
            .LoadChunkDecompressed(meshData.Skeleton)
            .AsStream();

        using SkinnedMesh skinnedMesh = SkinnedMesh.ReadFromSimpleSkin(simpleSkinStream);
        RigResource skeleton = new(skeletonStream);

        return skinnedMesh.ToGltf(
            skeleton,
            CollectMaterialTextures(
                skinnedMesh,
                meshData,
                skinPackage,
                this.WadTab.Wad,
                metaEnvironment
            ),
            new List<(string, IAnimationAsset)>()
        );
    }

    private List<(string, Stream)> CollectMaterialTextures(
        SkinnedMesh skinnedMesh,
        SkinMeshDataProperties meshData,
        BinTree skinPackage,
        WadFile wad,
        MetaEnvironment metaEnvironment
    )
    {
        string defaultTexture = ResolveMaterialTexturePath(
            meshData.Material,
            meshData.Texture,
            skinPackage,
            metaEnvironment
        );
        List<(string, Stream)> textures = new();

        foreach (SkinnedMeshRange primitive in skinnedMesh.Ranges)
        {
            SkinMeshDataProperties_MaterialOverride materialOverride =
                meshData.MaterialOverride.FirstOrDefault(
                    x => x.Value.Submesh == primitive.Material
                );

            textures.Add(
                (materialOverride is null) switch
                {
                    true => (primitive.Material, CreateTextureImage(defaultTexture, wad)),
                    false
                        => (
                            primitive.Material,
                            CreateMaterialTextureImage(
                                materialOverride.Material,
                                materialOverride.Texture,
                                skinPackage,
                                wad,
                                metaEnvironment
                            )
                        )
                }
            );
        }

        return textures;
    }

    private static Stream CreateMaterialTextureImage(
        MetaObjectLink materialLink,
        string fallbackTexturePath,
        BinTree skinPackage,
        WadFile wad,
        MetaEnvironment metaEnvironment
    )
    {
        BinTreeObject materialDefObject = skinPackage.Objects.GetValueOrDefault(materialLink);
        if (materialDefObject is null)
            return CreateTextureImage(fallbackTexturePath, wad);

        var materialDef = MetaSerializer.Deserialize<StaticMaterialDef>(
            metaEnvironment,
            materialDefObject
        );
        StaticMaterialShaderSamplerDef diffuseSamplerDef = materialDef.SamplerValues.FirstOrDefault(
            x => DIFFUSE_SAMPLERS.Contains(x.Value.SamplerName)
        );
        diffuseSamplerDef ??= new();

        return string.IsNullOrEmpty(diffuseSamplerDef.TextureName) switch
        {
            true => CreateTextureImage(fallbackTexturePath, wad),
            false => CreateTextureImage(diffuseSamplerDef.TextureName, wad),
        };
    }

    private static string ResolveMaterialTexturePath(
        MetaObjectLink materialLink,
        string fallbackTexturePath,
        BinTree skinPackage,
        MetaEnvironment metaEnvironment
    )
    {
        BinTreeObject materialDefObject = skinPackage.Objects.GetValueOrDefault(materialLink);
        if (materialDefObject is null)
            return fallbackTexturePath;

        var materialDef = MetaSerializer.Deserialize<StaticMaterialDef>(
            metaEnvironment,
            materialDefObject
        );
        StaticMaterialShaderSamplerDef diffuseSamplerDef = materialDef.SamplerValues.FirstOrDefault(
            x => DIFFUSE_SAMPLERS.Contains(x.Value.SamplerName)
        );
        diffuseSamplerDef ??= new();

        return string.IsNullOrEmpty(diffuseSamplerDef.TextureName) switch
        {
            true => fallbackTexturePath,
            false => diffuseSamplerDef.TextureName,
        };
    }

    private static Stream CreateTextureImage(string path, WadFile wad)
    {
        using Stream fallbackTextureStream = wad.LoadChunkDecompressed(path).AsStream();
        return ImageUtils.ConvertTextureToPng(Texture.Load(fallbackTextureStream));
    }

    private void ToggleIsSavingAsGltf(bool value)
    {
        this._isSavingAsGltf = value;
        StateHasChanged();
    }
}
