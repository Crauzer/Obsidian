using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public abstract class WadItemModel: IComparable<WadItemModel>
{
    public WadFolderModel Parent { get; protected set; }
    
    public string Name { get; set; }

    public bool IsChecked { get; set; }

    public int CompareTo(WadItemModel other) => (this, other) switch
    {
        (WadFolderModel, WadFileModel) => -1,
        (WadFileModel, WadFolderModel) => 1,
        _ => this.Name.CompareTo(other.Name)
    };
}
