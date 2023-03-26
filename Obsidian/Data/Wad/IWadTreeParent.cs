using LeagueToolkit.Core.Wad;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Obsidian.Data.Wad;

public interface IWadTreeParent : IWadTreePathable
{
    HashSet<WadTreeItemModel> Items { get; }
}

public static class IWadTreeParentExtensions
{
    public static WadTreeItemModel PrepareDirectory(
        this IWadTreeParent parent,
        IEnumerable<string> pathComponents
    )
    {
        string folderName = pathComponents.First();
        WadTreeItemModel directory = parent.Items.FirstOrDefault(
            x => x.Type is WadTreeItemType.Directory && x.Name == folderName
        );

        directory ??= new(null, folderName);
        if (pathComponents.Count() is 1)
        {
            parent.Items.Add(directory);
        }
        else
        {
            directory.PrepareDirectory(pathComponents.Skip(1));
        }

        return directory;
    }

    public static void AddFsFile(this IWadTreeParent parent, IEnumerable<string> pathComponents)
    {
        // File belongs to this folder
        if (pathComponents.Count() is 1)
        {
            parent.Items.Add(new WadTreeItemModel(parent, pathComponents.First()));
            return;
        }

        string directoryName = pathComponents.First();
        WadTreeItemModel directory = parent.Items.FirstOrDefault(
            x => x.Type is WadTreeItemType.Directory && x.Name == directoryName
        );

        if (directory is null)
        {
            directory = new(parent, directoryName);
            parent.Items.Add(directory);
        }

        directory.AddFsFile(pathComponents.Skip(1));
    }

    public static void AddWadFile(
        this IWadTreeParent parent,
        IEnumerable<string> pathComponents,
        WadFile wad,
        WadChunk chunk
    )
    {
        // File belongs to this folder
        if (pathComponents.Count() is 1)
        {
            parent.Items.Add(new WadTreeFileModel(parent, pathComponents.First(), wad, chunk));
            return;
        }

        string folderName = pathComponents.First();
        WadTreeItemModel directory = parent.Items.FirstOrDefault(
            x => x.Type is WadTreeItemType.Directory && x.Name == folderName
        );

        if (directory is null)
        {
            directory = new(parent, folderName);
            parent.Items.Add(directory);
        }

        directory.AddWadFile(pathComponents.Skip(1), wad, chunk);
    }

    public static WadTreeItemModel FindItemOrDefault(this IWadTreeParent parent, string path) =>
        parent.TraverseFlattenedItems().FirstOrDefault(x => x.Path == path);

    public static IEnumerable<WadTreeItemModel> TraverseFlattenedItems(this IWadTreeParent parent)
    {
        if (parent.Items is null)
            yield break;

        foreach (WadTreeItemModel item in parent.Items)
        {
            yield return item;

            foreach (WadTreeItemModel itemItem in item.TraverseFlattenedItems())
                yield return itemItem;
        }
    }

    public static IEnumerable<WadTreeItemModel> TraverseFlattenedCheckedItems(
        this IWadTreeParent parent
    )
    {
        if (parent.Items is null)
            yield break;

        foreach (WadTreeItemModel item in parent.Items)
        {
            if (item.IsChecked)
                yield return item;

            foreach (WadTreeItemModel itemItem in item.TraverseFlattenedCheckedItems())
                yield return itemItem;
        }
    }

    public static IEnumerable<WadTreeItemModel> TraverseFlattenedVisibleItems(
        this IWadTreeParent parent,
        string filter,
        bool useRegex = false
    )
    {
        if (parent.Items is null)
            yield break;

        foreach (WadTreeItemModel item in parent.Items)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                // If the current item is a file we check if it matches the filter
                if (item is WadTreeFileModel && DoesMatchFilter(item, filter, useRegex))
                {
                    yield return item;
                    continue;
                }

                // If the current item is a folder we get filtered items and if there are none we skip
                IReadOnlyList<WadTreeItemModel> filteredItems = item.TraverseFlattenedVisibleItems(
                        filter,
                        useRegex
                    )
                    .ToList();
                if (filteredItems.Count is 0)
                    continue;

                // Return parent only if its children are included in the filter
                yield return item;

                if (item.IsExpanded)
                    foreach (WadTreeItemModel itemItem in filteredItems)
                        yield return itemItem;
            }
            else
            {
                // root items are always visible
                yield return item;

                if (item.Type is WadTreeItemType.Directory && item.IsExpanded)
                    foreach (WadTreeItemModel itemItem in item.TraverseFlattenedVisibleItems(null))
                        yield return itemItem;
            }
        }
    }

    public static bool DoesMatchFilter(WadTreeItemModel item, string filter, bool useRegex) =>
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
}
