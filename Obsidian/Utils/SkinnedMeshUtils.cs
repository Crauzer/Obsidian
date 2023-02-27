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
}
