using System.Numerics;

namespace Obsidian.BabylonJs;

public struct ThreeBone
{
    public string Name { get; set; }

    public int Id { get; set; }
    public int ParentId { get; set; }

    public float[] LocalTransform { get; set; }
    public float[] InverseBindTransform { get; set; }
}
