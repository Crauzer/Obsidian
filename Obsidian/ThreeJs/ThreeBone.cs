using System.Numerics;

namespace Obsidian.BabylonJs;

public struct ThreeBone
{
    public string Name { get; set; }

    public int Id { get; set; }
    public int ParentId { get; set; }

    public float[] LocalTranslation { get; set; }
    public float[] LocalRotation { get; set; }
    public float[] LocalScale { get; set; }

    public float[] InverseBindTranslation { get; set; }
    public float[] InverseBindRotation { get; set; }
    public float[] InverseBindScale { get; set; }
}
