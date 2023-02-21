using System.Diagnostics;
using System.Windows.Controls;
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

    public bool IsSelected { get; set; }

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
    public IEnumerable<WadItemModel> TraverseFlattenedSelectedItems()
    {
        if (this.Items is null)
            yield break;

        foreach (WadItemModel item in this.Items)
        {
            if (item.IsSelected)
                yield return item;

            foreach (WadItemModel itemItem in item.TraverseFlattenedSelectedItems())
                yield return itemItem;
        }
    }
    public IEnumerable<WadItemModel> TraverseFlattenedVisibleItems()
    {
        if (this.Items is null)
            yield break;

        foreach (WadItemModel item in this.Items)
        {
            // root items are always visible
            yield return item;

            if (item is WadFolderModel folder && item.IsExpanded)
                foreach (WadItemModel folderItem in folder.TraverseFlattenedVisibleItems())
                    yield return folderItem;
        }
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
