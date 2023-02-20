using LeagueToolkit.Core.Wad;
using System.Diagnostics;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public sealed class WadFileModel : WadItemModel
{
    public WadChunk Chunk { get; }

    public WadFileModel(WadFolderModel parent, string name, WadChunk chunk)
    {
        this.Parent = parent;
        this.Name = name;
        this.Chunk = chunk;
    }
}
