using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Meta.Classes;
using LeagueToolkit.Meta;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueToolkit.Core.Animation;
using CommunityToolkit.HighPerformance;

namespace Obsidian.Utils;

public static class SkinnedMeshUtils
{
    private static readonly string[] DIFFUSE_SAMPLERS = new[]
    {
        "DiffuseTexture",
        "Diffuse_Texture"
    };

    public static async Task<Dictionary<string, string>> CreateTextureImages(
        IJSRuntime js,
        SkinnedMesh skinnedMesh,
        SkinMeshDataProperties meshData,
        BinTree skinPackage,
        WadFile wad,
        MetaEnvironment metaEnvironment
    )
    {
        string defaultTexture = await CreateMaterialTextureImageBlob(
            meshData.Material,
            meshData.Texture,
            skinPackage,
            wad,
            metaEnvironment,
            js
        );
        Dictionary<string, string> textures = skinnedMesh.Ranges.ToDictionary(
            x => x.Material,
            _ => defaultTexture
        );

        foreach (
            SkinMeshDataProperties_MaterialOverride materialOverride in meshData.MaterialOverride
        )
        {
            if (textures.ContainsKey(materialOverride.Submesh) is false)
                continue;

            if (string.IsNullOrEmpty(materialOverride.Texture) is false)
            {
                textures[materialOverride.Submesh] = await ImageUtils.CreateImageBlobFromChunk(
                    js,
                    materialOverride.Texture,
                    wad
                );
                continue;
            }

            textures[materialOverride.Submesh] = await CreateMaterialTextureImageBlob(
                materialOverride.Material,
                defaultTexture,
                skinPackage,
                wad,
                metaEnvironment,
                js
            );
        }

        return textures;
    }

    public static IEnumerable<(string, IAnimationAsset)> LoadAnimationAssets(
        SkinCharacterDataProperties characterData,
        BinTree skinPackage,
        WadFile wad,
        MetaEnvironment metaEnvironment
    )
    {
        string animationsPath = skinPackage.Dependencies.FirstOrDefault(
            x => BinUtils.IsSkinAnimations(x)
        );
        if (string.IsNullOrEmpty(animationsPath))
            yield break;

        // Load animations bin
        using Stream animationsStream = wad.LoadChunkDecompressed(animationsPath).AsStream();
        BinTree animationsBin = new(animationsStream);

        // Resolve animation graph data object
        BinTreeObject animationGraphDataObject = animationsBin.Objects.GetValueOrDefault(
            characterData.SkinAnimationProperties.Value.AnimationGraphData
        );
        if (animationGraphDataObject is null)
            yield break;

        // Serialize animation graph data
        var animationGraphData = MetaSerializer.Deserialize<AnimationGraphData>(
            metaEnvironment,
            animationGraphDataObject
        );

        foreach (var (_, clipData) in animationGraphData.ClipDataMap)
        {
            if (clipData is not AtomicClipData atomicClipData)
                continue;

            string name = Path.GetFileNameWithoutExtension(
                atomicClipData.AnimationResourceData.Value.AnimationFilePath
            );
            using Stream assetStream = wad.LoadChunkDecompressed(
                    atomicClipData.AnimationResourceData.Value.AnimationFilePath
                )
                .AsStream();

            yield return (name, AnimationAsset.Load(assetStream));
        }
    }

    private static async Task<string> CreateMaterialTextureImageBlob(
        MetaObjectLink materialLink,
        string fallbackTexture,
        BinTree skinPackage,
        WadFile wad,
        MetaEnvironment metaEnvironment,
        IJSRuntime js
    )
    {
        BinTreeObject materialDefObject = skinPackage.Objects.GetValueOrDefault(materialLink);
        if (materialDefObject is null)
            return await ImageUtils.CreateImageBlobFromChunk(js, fallbackTexture, wad);

        var materialDef = MetaSerializer.Deserialize<StaticMaterialDef>(
            metaEnvironment,
            materialDefObject
        );
        StaticMaterialShaderSamplerDef diffuseSamplerDef = materialDef.SamplerValues.FirstOrDefault(
            x => DIFFUSE_SAMPLERS.Contains(x.Value.SamplerName)
        );
        diffuseSamplerDef ??= new();

        if (string.IsNullOrEmpty(diffuseSamplerDef.TextureName) is false)
            return await ImageUtils.CreateImageBlobFromChunk(
                js,
                diffuseSamplerDef.TextureName,
                wad
            );

        return await ImageUtils.CreateImageBlobFromChunk(js, fallbackTexture, wad);
    }

    public static List<(string, Stream)> CollectMaterialTextures(
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
                    true
                        => (
                            primitive.Material,
                            ImageUtils.CreateTexturePngImage(defaultTexture, wad)
                        ),
                    false
                        => (
                            primitive.Material,
                            CreateMaterialTextureImage(
                                materialOverride.Material,
                                defaultTexture,
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
            return ImageUtils.CreateTexturePngImage(fallbackTexturePath, wad);

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
            true => ImageUtils.CreateTexturePngImage(fallbackTexturePath, wad),
            false => ImageUtils.CreateTexturePngImage(diffuseSamplerDef.TextureName, wad),
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
}
