using LeagueToolkit.Utils;
using MudBlazor;
using System.Diagnostics;
using System.Text.RegularExpressions;
using PathIO = System.IO.Path;


namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public abstract class WadItemModel : IComparable<WadItemModel>
{
    public WadItemType Type =>
        this switch
        {
            WadFileModel => WadItemType.File,
            WadFolderModel => WadItemType.Folder,
            _ => throw new NotImplementedException("Invalid wad item type")
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

    public string Icon => GetIcon();

    public bool IsSelected { get; set; }
    public bool IsChecked { get; set; }
    public bool IsExpanded { get; set; }

    public HashSet<WadItemModel> Items { get; protected set; }

    public void UpdateSelectedItems() { }

    public void SortItems()
    {
        if (this.Items is null)
            return;

        this.Items = new(this.Items.OrderBy(x => x));

        foreach (WadItemModel item in this.Items)
        {
            if (item is WadFolderModel folder)
                folder.SortItems();
        }
    }

    public IEnumerable<WadItemModel> TraverseFlattenedItems()
    {
        if (this.Items is null)
            yield break;

        foreach (WadItemModel item in this.Items)
        {
            yield return item;

            foreach (WadItemModel itemItem in item.TraverseFlattenedItems())
                yield return itemItem;
        }
    }

    public IEnumerable<WadItemModel> TraverseFlattenedCheckedItems()
    {
        if (this.Items is null)
            yield break;

        foreach (WadItemModel item in this.Items)
        {
            if (item.IsChecked)
                yield return item;

            foreach (WadItemModel itemItem in item.TraverseFlattenedCheckedItems())
                yield return itemItem;
        }
    }

    public IEnumerable<WadItemModel> TraverseFlattenedVisibleItems(
        string filter,
        bool useRegex = false
    )
    {
        if (this.Items is null)
            yield break;

        foreach (WadItemModel item in this.Items)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                // If the current item is a file we check if it matches the filter
                if (item is WadFileModel && DoesMatchFilter(item, filter, useRegex))
                {
                    yield return item;
                    continue;
                }

                // If the current item is a folder we get filtered items and if there are none we skip
                IReadOnlyList<WadItemModel> filteredItems = item.TraverseFlattenedVisibleItems(
                        filter,
                        useRegex
                    )
                    .ToList();
                if (filteredItems.Count is 0)
                    continue;

                // Return parent only if its children are included in the filter
                yield return item;

                if (item.IsExpanded)
                    foreach (WadItemModel itemItem in filteredItems)
                        yield return itemItem;
            }
            else
            {
                // root items are always visible
                yield return item;

                if (item is WadFolderModel folder && item.IsExpanded)
                    foreach (WadItemModel folderItem in folder.TraverseFlattenedVisibleItems(null))
                        yield return folderItem;
            }
        }
    }

    public static bool DoesMatchFilter(WadItemModel item, string filter, bool useRegex) =>
        useRegex switch
        {
            true
                => Regex.IsMatch(
                    item.Path,
                    filter,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
                ),
            false => item.Path.Contains(filter, StringComparison.InvariantCultureIgnoreCase)
        };

    public string GetIcon()
    {
        if (this.Type is WadItemType.Folder)
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
