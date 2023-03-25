using Serilog;

namespace Obsidian.Data.FileTree;

public sealed class FileTreeModel
{
    public HashSet<FileTreeItemModel> Items { get; set; } = new();

    public FileTreeModel(IEnumerable<string> files) => Rebuild(files);

    private void Rebuild(IEnumerable<string> files)
    {
        Log.Information("Rebuilding file tree");

        foreach(string file in files)
        {
            string[] pathComponents = file.Split(Path.DirectorySeparatorChar);

            if (pathComponents.Length is 1)
            {
                this.Items.Add(new(null, file));
            }
            else
            {
                string directoryName = pathComponents.First();
                FileTreeItemModel directory = this.Items.FirstOrDefault(
                    x => x.Type is FileTreeItemType.Directory && x.Name == directoryName
                );

                // Create directory if it doesn't exist yet
                if (directory is null)
                {
                    directory = new(null, directoryName);
                    this.Items.Add(directory);
                }

                directory.AddFile(pathComponents.Skip(1));
            }
        }

        SortItems();
    }

    public IEnumerable<FileTreeItemModel> TraverseItems()
    {
        foreach(FileTreeItemModel item in this.Items)
        {
            yield return item;

            foreach(FileTreeItemModel itemChild in item.TraverseItems())
                yield return itemChild;
        }
    }

    public void SortItems()
    {
        Log.Information("Sorting file tree");

        if (this.Items is null)
            return;

        this.Items = new(this.Items.OrderBy(x => x));

        foreach (FileTreeItemModel item in this.Items)
            item.SortItems();
    }
}
