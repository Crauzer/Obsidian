using CommunityToolkit.Diagnostics;
using LeagueToolkit.Core.Wad;
using MudBlazor;
using Obsidian.Data.Wad;
using System.Windows.Controls;

namespace Obsidian.Data.FileTree;

public class FileTreeItemModel : IComparable<FileTreeItemModel>
{
    public FileTreeItemType Type =>
        this.Items switch
        {
            { Count: 0 } => FileTreeItemType.File,
            { Count: > 0 } => FileTreeItemType.Directory,
            _ => throw new InvalidOperationException("Invalid file tree item type")
        };

    public Guid Id { get; } = Guid.NewGuid();

    public FileTreeItemModel Parent { get; protected set; }

    public string Icon => GetIcon();

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

    public HashSet<FileTreeItemModel> Items { get; protected set; } = new();

    public int Depth
    {
        get
        {
            if (this.Parent is null)
                return 0;

            return this.Parent.Depth + 1;
        }
    }

    public bool IsSelected { get; set; }
    public bool IsChecked { get; set; }
    public bool IsExpanded { get; set; }

    public FileTreeItemModel(FileTreeItemModel parent, string name)
    {
        Guard.IsNotNullOrEmpty(name, nameof(name));

        this.Parent = parent;
        this.Name = name;
    }

    public void AddFile(IEnumerable<string> pathComponents)
    {
        // File belongs to this folder
        if (pathComponents.Count() is 1)
        {
            this.Items.Add(new(this, pathComponents.First()));
            return;
        }

        string directoryName = pathComponents.First();
        FileTreeItemModel directory = this.Items.FirstOrDefault(
            x => x.Type is FileTreeItemType.Directory && x.Name == directoryName
        );

        // Create directory if it doesn't exist yet
        if (directory is null)
        {
            directory = new(this, directoryName);
            this.Items.Add(directory);
        }

        directory.AddFile(pathComponents.Skip(1));
    }

    public IEnumerable<FileTreeItemModel> TraverseItems()
    {
        foreach (FileTreeItemModel item in this.Items)
        {
            yield return item;

            foreach (FileTreeItemModel itemChild in item.TraverseItems())
                yield return itemChild;
        }
    }

    public void SortItems()
    {
        if (this.Items is null)
            return;

        this.Items = new(this.Items.OrderBy(x => x));

        foreach (FileTreeItemModel item in this.Items)
            item.SortItems();
    }

    public string GetIcon()
    {
        if(this.Items.Count is 0)
            return Icons.Material.TwoTone.Archive;

        return Icons.Material.TwoTone.Folder;
    }

    public int CompareTo(FileTreeItemModel other) =>
        (this.Type, other.Type) switch
        {
            (FileTreeItemType.Directory, FileTreeItemType.File) => -1,
            (FileTreeItemType.File, FileTreeItemType.Directory) => 1,
            _ => this.Name.CompareTo(other.Name)
        };
}

public enum FileTreeItemType
{
    File,
    Directory
}
