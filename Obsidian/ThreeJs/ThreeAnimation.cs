using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.ThreeJs;

public struct ThreeAnimation
{
    public string Name { get; set; }
    public Dictionary<string, ThreeAnimationClip> Clips { get; set; }
}

public struct ThreeAnimationClip
{
    public string JointName { get; set; }
    public ThreeAnimationTrack Translations { get; set; }
    public ThreeAnimationTrack Rotations { get; set; }
    public ThreeAnimationTrack Scales { get; set; }
}

public struct ThreeAnimationTrack
{
    public float[] KeyTimes { get; set; }
    public float[] Values { get; set; }
}
