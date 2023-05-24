using LeagueToolkit.Core.Wad;
using LeagueToolkit.Utils;
using MudBlazor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using PathIO = System.IO.Path;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public class WadTreeItemModel
    : IWadTreePathable,
        IWadTreeParent,
        IComparable<WadTreeItemModel>,
        IEquatable<WadTreeItemModel> {
    public WadTreeItemType Type =>
        this.Items switch { { Count: 0 } => WadTreeItemType.File, { Count: > 0 } => WadTreeItemType.Directory,
            _ => throw new InvalidOperationException("Invalid wad tree item type")
        };

    public IWadTreeParent Parent { get; protected set; }
    public int Depth => this.GetDepth();

    public Guid Id { get; } = Guid.NewGuid();

    private string _name;
    public string Name {
        get => this._name;
        set {
            this._name = value;

            this.IsWadArchive = this._name.EndsWith(".wad", StringComparison.OrdinalIgnoreCase)
                                || this._name.EndsWith(".wad.client", StringComparison.OrdinalIgnoreCase)
                                || this._name.EndsWith(".wad.mobile", StringComparison.OrdinalIgnoreCase);
        }
    }

    public string Path { get; }

    public string Icon => GetIcon();

    public bool IsHighlighted => this.IsWadArchive;

    public bool IsSelected { get; set; }
    public bool IsChecked { get; set; }
    public bool IsExpanded { get; set; }

    public List<WadTreeItemModel> Items { get; protected set; } = new();

    public bool IsWadArchive { get; private set; }

    public WadTreeItemModel(IWadTreeParent parent, string name) {
        this.Parent = parent;
        this.Name = name;

        this.Path = parent switch {
            null or WadTreeModel or { IsWadArchive: true } => name,
            _ => string.Join('/', parent.Path, name)
        };
    }

    public void SortItems() {
        if (this.Items is null)
            return;

        this.Items.Sort();

        foreach (WadTreeItemModel item in this.Items.Where(item => item.Type is WadTreeItemType.Directory)) {
            item.SortItems();
        }
    }

    public void CheckItemTree(bool value) {
        if (this.Items is not null)
            foreach (WadTreeItemModel item in this.TraverseFlattenedItems())
                item.IsChecked = value;
    }

    public string GetIcon() {
        string extension = PathIO.GetExtension(this.Name);
        if (this.IsWadArchive)
            return Icons.Material.TwoTone.Archive;

        if (this.Type is WadTreeItemType.Directory)
            return Icons.Material.TwoTone.Folder;

        LeagueFileType fileType = LeagueFile.GetFileType(extension);
        return fileType switch {
            LeagueFileType.Animation => Icons.Material.TwoTone.Animation,
            LeagueFileType.Jpeg => Icons.Material.TwoTone.Image,
            LeagueFileType.MapGeometry => CustomIcons.Material.ImageFilterHdr,
            LeagueFileType.Png => Icons.Material.TwoTone.Image,
            LeagueFileType.PropertyBin => CustomIcons.Material.CodeBracesBox,
            LeagueFileType.PropertyBinOverride => CustomIcons.Material.CodeBracesBox,
            LeagueFileType.RiotStringTable => Icons.Material.TwoTone.Translate,
            LeagueFileType.SimpleSkin => CustomIcons.Material.Cube,
            LeagueFileType.Skeleton => CustomIcons.Material.Bone,
            LeagueFileType.StaticMeshAscii => CustomIcons.Material.Cube,
            LeagueFileType.StaticMeshBinary => CustomIcons.Material.Cube,
            LeagueFileType.Texture => Icons.Material.TwoTone.Image,
            LeagueFileType.TextureDds => Icons.Material.TwoTone.Image,
            LeagueFileType.WorldGeometry => CustomIcons.Material.ImageFilterHdr,
            LeagueFileType.WadArchive => Icons.Material.TwoTone.Archive,
            LeagueFileType.WwiseBank => CustomIcons.Material.VolumeHigh,
            LeagueFileType.WwisePackage => CustomIcons.Material.AccountVoice,
            _ => Icons.Custom.FileFormats.FileDocument,
        };
    }

    // beautiful
    public int CompareTo(WadTreeItemModel other) =>
        (this.IsWadArchive, other.IsWadArchive) switch {
            (true, true) => this.Name.CompareTo(other?.Name),
            (true, false) => 1,
            (false, true) => -1,
            (false, false)
                => (this.Type, other?.Type) switch {
                    (WadTreeItemType.Directory, WadTreeItemType.File) => -1,
                    (WadTreeItemType.File, WadTreeItemType.Directory) => 1,
                    _ => this.Name.CompareTo(other?.Name)
                }
        };

    public bool Equals(WadTreeItemModel other) => this.Id == other?.Id;

    public override bool Equals(object obj) =>
        obj switch {
            WadTreeItemModel item => Equals(item),
            _ => false
        };

    public override int GetHashCode() => this.Id.GetHashCode();

    #region Operator overloads
    public static bool operator ==(WadTreeItemModel left, WadTreeItemModel right) =>
        left.Equals(right);

    public static bool operator !=(WadTreeItemModel left, WadTreeItemModel right) =>
        !left.Equals(right);

    public static bool operator <(WadTreeItemModel left, WadTreeItemModel right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(WadTreeItemModel left, WadTreeItemModel right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(WadTreeItemModel left, WadTreeItemModel right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(WadTreeItemModel left, WadTreeItemModel right) =>
        left.CompareTo(right) >= 0;
    #endregion
}

public enum WadTreeItemType {
    File,
    Directory
}