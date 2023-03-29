using LeagueToolkit.Core.Wad;
using System.Diagnostics;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public sealed class WadTreeFileModel : WadTreeItemModel
{
    public WadChunk Chunk { get; }
    public WadFile Wad { get; }

    public WadTreeFileModel(IWadTreeParent parent, string name, WadFile wad, WadChunk chunk)
        : base(parent, name)
    {
        this.Chunk = chunk;
        this.Wad = wad;
    }
}
