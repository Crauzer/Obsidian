using System.Diagnostics;
using PathIO = System.IO.Path;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public abstract class WadItemModel : IComparable<WadItemModel>
{
    public WadItemType Type =>
        this switch
        {
            WadFileModel => WadItemType.File,
            WadFolderModel => WadItemType.Folder
        };

    public WadFolderModel Parent { get; protected set; }
    public int Depth
    {
        get
        {
            if (this.Parent is null)
                return 0;

            return this.Parent.Depth + 1;
        }
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Path
    {
        get
        {
            if (this.Parent is null)
                return this.Name;

            return string.Join('/', this.Parent.Path, this.Name);
        }
    }

    public bool IsChecked { get; set; }
    public bool IsExpanded { get; set; }

    public HashSet<WadItemModel> Items { get; protected set; }

    public int CompareTo(WadItemModel other) =>
        (this, other) switch
        {
            (WadFolderModel, WadFileModel) => -1,
            (WadFileModel, WadFolderModel) => 1,
            _ => this.Name.CompareTo(other.Name)
        };
}

public enum WadItemType
{
    File,
    Folder
}
