using LeagueToolkit.Core.Wad;
using LeagueToolkit.Utils;
using MudBlazor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using PathIO = System.IO.Path;

namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public class WadTreeItemModel : IWadTreePathable, IWadTreeParent, IComparable<WadTreeItemModel>
{
    public WadTreeItemType Type =>
        this.Items switch
        {
            { Count: 0 } => WadTreeItemType.File,
            { Count: > 0 } => WadTreeItemType.Directory,
            _ => throw new InvalidOperationException("Invalid file tree item type")
        };

    public IWadTreePathable Parent { get; protected set; }
    public int Depth => this.GetDepth();

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Path => this.GetPath();

    public string Icon => GetIcon();

    public bool IsSelected { get; set; }
    public bool IsChecked { get; set; }
    public bool IsExpanded { get; set; }

    public HashSet<WadTreeItemModel> Items { get; protected set; } = new();

    public WadTreeItemModel(IWadTreePathable parent, string name)
    {
        this.Parent = parent;
        this.Name = name;
    }

    

    public void AddWadFile(IEnumerable<string> pathComponents, WadFile wad, WadChunk chunk)
    {
        // File belongs to this folder
        if (pathComponents.Count() is 1)
        {
            this.Items.Add(new WadTreeFileModel(this, pathComponents.First(), wad, chunk));
            return;
        }

        string folderName = pathComponents.First();
        WadTreeItemModel directory = this.Items.FirstOrDefault(
            x => x.Type is WadTreeItemType.Directory && x.Name == folderName
        );

        if (directory is null)
        {
            directory = new(this, folderName);
            this.Items.Add(directory);
        }

        directory.AddWadFile(pathComponents.Skip(1), wad, chunk);
    }

    public void SortItems()
    {
        if (this.Items is null)
            return;

        this.Items = new(this.Items.OrderBy(x => x));

        foreach (WadTreeItemModel item in this.Items)
        {
            if (item.Type is WadTreeItemType.Directory)
                item.SortItems();
        }
    }

    public void CheckItemTree(bool value)
    {
        if (this.Items is not null)
            foreach (WadTreeItemModel item in this.TraverseFlattenedItems())
                item.IsChecked = value;
    }

    public string GetIcon()
    {
        if (this.Type is WadTreeItemType.Directory)
            return Icons.Material.TwoTone.Folder;

        LeagueFileType fileType = LeagueFile.GetFileType(PathIO.GetExtension(this.Name));
        return fileType switch
        {
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
            LeagueFileType.WwiseBank => CustomIcons.Material.VolumeHigh,
            LeagueFileType.WwisePackage => CustomIcons.Material.AccountVoice,
            _ => Icons.Custom.FileFormats.FileDocument,
        };
    }

    public int CompareTo(WadTreeItemModel other) =>
        (this.Type, other.Type) switch
        {
            (WadTreeItemType.Directory, WadTreeItemType.File) => -1,
            (WadTreeItemType.File, WadTreeItemType.Directory) => 1,
            _ => this.Name.CompareTo(other.Name)
        };
}

public enum WadTreeItemType
{
    File,
    Directory
}
