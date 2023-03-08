using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Memory;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Hashing;
using LeagueToolkit.IO.SimpleSkinFile;
using Microsoft.JSInterop;
using Obsidian.ThreeJs;
using System.Numerics;

namespace Obsidian.BabylonJs;

public static class Three
{
    public static async Task InitializeViewport(IJSRuntime js, string viewportId) =>
        await js.InvokeVoidAsync("initThreeJsRenderer", viewportId);

    public static async Task RenderSkinnedMesh(
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

        await InitializeViewport(js, canvasId);
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

    public static async Task RenderSkinnedMeshFromGltf(
        IJSRuntime js,
        string viewportId,
        SkinnedMesh skinnedMesh,
        RigResource skeleton,
        IEnumerable<(string, Stream)> textures,
        IEnumerable<(string, IAnimationAsset)> animations
    )
    {
        MemoryStream gltfStream = new();
        await Task.Run(() =>
        {
            skinnedMesh.ToGltf(skeleton, textures, animations).WriteGLB(gltfStream);
            gltfStream.Position = 0;
        });

        string gltfBlob = await js.InvokeAsync<string>(
            "createBlobFromStream",
            new DotNetStreamReference(gltfStream)
        );

        await InitializeViewport(js, viewportId);
        await js.InvokeVoidAsync("renderSkinnedMeshFromGltf", viewportId, gltfBlob);
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

    private static ThreeAnimation[] CreateAnimations(
        IEnumerable<(string Name, IAnimationAsset Asset)> animations,
        RigResource skeleton
    )
    {
        return animations.Select(x => CreateAnimation(x.Name, x.Asset, skeleton)).ToArray();
    }

    private static ThreeAnimation CreateAnimation(
        string name,
        IAnimationAsset asset,
        RigResource skeleton
    )
    {
        int frameCount = (int)(asset.Fps * asset.Duration);
        float frameDuration = 1 / asset.Fps;

        Dictionary<uint, string> jointNames =
            new(
                skeleton.Joints.Select(
                    x => new KeyValuePair<uint, string>(Elf.HashLower(x.Name), x.Name)
                )
            );
        Dictionary<uint, (Quaternion Rotation, Vector3 Translation, Vector3 Scale)> pose = new();

        Dictionary<uint, (float, Quaternion)[]> jointRotations = new();
        Dictionary<uint, (float, Vector3)[]> jointTranslations = new();
        Dictionary<uint, (float, Vector3)[]> jointScales = new();

        CreateEmptyQuaternionTracks(jointRotations, frameCount, skeleton);
        CreateEmptyVector3Tracks(jointTranslations, frameCount, skeleton);
        CreateEmptyVector3Tracks(jointScales, frameCount, skeleton);

        for (int frameId = 0; frameId < frameCount; frameId++)
        {
            float frameTime = frameId * frameDuration;

            asset.Evaluate(frameTime, pose);

            foreach (var (jointHash, transform) in pose)
            {
                if (!jointRotations.ContainsKey(jointHash))
                    jointRotations.Add(jointHash, new (float, Quaternion)[frameCount]);
                jointRotations[jointHash][frameId] = (frameTime, transform.Rotation);

                if (!jointTranslations.ContainsKey(jointHash))
                    jointTranslations.Add(jointHash, new (float, Vector3)[frameCount]);
                jointTranslations[jointHash][frameId] = (frameTime, transform.Translation);

                if (!jointScales.ContainsKey(jointHash))
                    jointScales.Add(jointHash, new (float, Vector3)[frameCount]);
                jointScales[jointHash][frameId] = (frameTime, transform.Scale);
            }
        }

        Dictionary<string, ThreeAnimationClip> clips = new();
        foreach (var (jointHash, jointName) in jointNames)
        {
            ThreeAnimationTrack translationTrack = new();
            ThreeAnimationTrack rotationTrack = new();
            ThreeAnimationTrack scaleTrack = new();

            if (jointTranslations.TryGetValue(jointHash, out var translations))
            {
                translationTrack.KeyTimes = translations.Select(x => x.Item1).ToArray();
                translationTrack.Values = FlattenVector3Collection(
                        translations.Select(x => x.Item2)
                    )
                    .ToArray();
            }
            if (jointRotations.TryGetValue(jointHash, out var rotations))
            {
                rotationTrack.KeyTimes = rotations.Select(x => x.Item1).ToArray();
                rotationTrack.Values = FlattenQuaternionCollection(rotations.Select(x => x.Item2))
                    .ToArray();
            }
            if (jointScales.TryGetValue(jointHash, out var scales))
            {
                scaleTrack.KeyTimes = scales.Select(x => x.Item1).ToArray();
                scaleTrack.Values = FlattenVector3Collection(scales.Select(x => x.Item2)).ToArray();
            }

            clips.Add(
                jointName,
                new()
                {
                    JointName = jointName,
                    Translations = translationTrack,
                    Rotations = rotationTrack,
                    Scales = scaleTrack
                }
            );
        }

        return new() { Name = name, Clips = clips };
    }

    private static void CreateEmptyVector3Tracks(
        Dictionary<uint, (float, Vector3)[]> tracks,
        int frameCount,
        RigResource skeleton
    )
    {
        foreach (Joint joint in skeleton.Joints)
            tracks.Add(Elf.HashLower(joint.Name), new (float, Vector3)[frameCount]);
    }

    private static void CreateEmptyQuaternionTracks(
        Dictionary<uint, (float, Quaternion)[]> tracks,
        int frameCount,
        RigResource skeleton
    )
    {
        foreach (Joint joint in skeleton.Joints)
            tracks.Add(Elf.HashLower(joint.Name), new (float, Quaternion)[frameCount]);
    }

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

    private static IEnumerable<float> FlattenVector3Collection(IEnumerable<Vector3> collection)
    {
        foreach (Vector3 value in collection)
        {
            yield return value.X;
            yield return value.Y;
            yield return value.Z;
        }
    }

    private static IEnumerable<float> FlattenQuaternionCollection(
        IEnumerable<Quaternion> collection
    )
    {
        foreach (Quaternion value in collection)
        {
            yield return value.X;
            yield return value.Y;
            yield return value.Z;
            yield return value.W;
        }
    }

    private static float[] CreateVector3(Vector3 value) =>
        new float[] { value.X, value.Y, value.Z };

    private static float[] CreateQuaternion(Quaternion value) =>
        new float[] { value.X, value.Y, value.Z, value.W };
}
