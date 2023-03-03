using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Memory;
using LeagueToolkit.Core.Mesh;
using Microsoft.JSInterop;
using System.Numerics;

namespace Obsidian.BabylonJs;

public static class Three
{
    public static async Task InitializeCanvas(IJSRuntime js, string canvasId) =>
        await js.InvokeVoidAsync("initThreeJsRenderer", canvasId);

    public static async Task CreateSkinnedMesh(
        IJSRuntime js,
        string canvasId,
        SkinnedMesh skinnedMesh,
        RigResource skeleton,
        IReadOnlyDictionary<string, string> textures
    )
    {
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
        int[] weightIds = CreateXyzwU8Data(
            skinnedMesh.VerticesView.GetAccessor(ElementName.BlendIndex).AsXyzwU8Array()
        );
        float[] weights = CreateVector4Data(
            skinnedMesh.VerticesView.GetAccessor(ElementName.BlendWeight).AsVector4Array()
        );

        uint[] indices = skinnedMesh.Indices.ToArray();

        ThreeBone[] bones = CreateBones(skeleton.Joints);

        await InitializeCanvas(js, canvasId);
        await js.InvokeVoidAsync(
            "renderSkinnedMesh",
            canvasId,
            bones,
            skinnedMesh.Ranges,
            indices,
            positions,
            normals,
            uvs,
            weightIds,
            weights,
            textures
        );
    }

    public static async Task ResizeEngine(IJSRuntime js, string canvasId) =>
        await js.InvokeVoidAsync("resizeBabylonEngine", canvasId);

    private static ThreeBone[] CreateBones(IEnumerable<Joint> joints) =>
        joints
            .Select(
                x =>
                    new ThreeBone
                    {
                        Name = x.Name,
                        Id = x.Id,
                        ParentId = x.ParentId,

                        LocalTranslation = CreateVector3(x.LocalTranslation),
                        LocalRotation = CreateQuaternion(x.LocalRotation),
                        LocalScale = CreateVector3(x.LocalScale),

                        InverseBindTranslation = CreateVector3(x.InverseBindTranslation),
                        InverseBindRotation = CreateQuaternion(x.InverseBindRotation),
                        InverseBindScale = CreateVector3(x.InverseBindScale),
                    }
            )
            .ToArray();

    private static float[] CreateVector2Data(IReadOnlyList<Vector2> array)
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

    private static float[] CreateVector3Data(IReadOnlyList<Vector3> array)
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

    private static float[] CreateVector4Data(IReadOnlyList<Vector4> array)
    {
        int dataOffset = 0;
        var data = new float[array.Count * 4];
        for (int i = 0; i < array.Count; i++)
        {
            data[dataOffset + 0] = array[i].X;
            data[dataOffset + 1] = array[i].Y;
            data[dataOffset + 2] = array[i].Z;
            data[dataOffset + 3] = array[i].W;

            dataOffset += 4;
        }

        return data;
    }

    private static int[] CreateXyzwU8Data(IReadOnlyList<(byte X, byte Y, byte Z, byte W)> array)
    {
        int dataOffset = 0;
        var data = new int[array.Count * 4];
        for (int i = 0; i < array.Count; i++)
        {
            data[dataOffset + 0] = array[i].X;
            data[dataOffset + 1] = array[i].Y;
            data[dataOffset + 2] = array[i].Z;
            data[dataOffset + 3] = array[i].W;

            dataOffset += 4;
        }

        return data;
    }

    private static float[] CreateUvData(IReadOnlyList<Vector2> array)
    {
        int dataOffset = 0;
        var data = new float[array.Count * 2];
        for (int i = 0; i < array.Count; i++)
        {
            data[dataOffset + 0] = array[i].X;
            data[dataOffset + 1] = 1 - array[i].Y; // babylon uses bottom left as origin

            dataOffset += 2;
        }

        return data;
    }

    private static float[] CreateVector3(Vector3 value) => new float[] { value.X, value.Y, value.Z };
    private static float[] CreateQuaternion(Quaternion value) => new float[] { value.X, value.Y, value.Z, value.W };
}
