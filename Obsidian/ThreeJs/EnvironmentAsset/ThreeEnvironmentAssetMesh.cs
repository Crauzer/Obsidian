namespace Obsidian.ThreeJs.EnvironmentAsset;

public struct ThreeEnvironmentAssetMesh
{
    public string Name { get; set; }

    public uint[] Indices { get; set; }

    public float[] Positions { get; set; }
    public float[] Normals { get; set; }
    public float[] Uvs { get; set; }
}
