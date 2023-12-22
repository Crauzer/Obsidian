using LeagueToolkit.Core.Wad;
using System.Diagnostics;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public sealed class WadTreeFileModel : WadTreeItemModel {
    public WadChunk Chunk { get; }

    public WadTreeFileModel(IWadTreeParent parent, string name, WadFile wad, WadChunk chunk)
        : base(parent, name, wad) {
        this.Chunk = chunk;
    }
}