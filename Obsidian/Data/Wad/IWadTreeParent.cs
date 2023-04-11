using LeagueToolkit.Core.Wad;
using System.Text.RegularExpressions;

namespace Obsidian.Data.Wad;

public interface IWadTreeParent : IWadTreePathable
{
    Dictionary<string, WadTreeItemModel> Items { get; }
}

public static class IWadTreeParentExtensions
{
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
            string name = pathComponents.First();
            parent.Items.Add(name, new WadTreeFileModel(parent, name, wad, chunk));
            return;
        }

        string folderName = pathComponents.First();
        WadTreeItemModel directory = null;
        lock (parent)
        {
            directory = parent.Items.GetValueOrDefault(folderName);
            if (directory is null)
            {
                directory = new(parent, folderName);
                parent.Items.Add(directory.Name, directory);
            }
        }

        directory.AddWadFile(pathComponents.Skip(1), wad, chunk);
    }

    public static IEnumerable<WadTreeItemModel> TraverseFlattenedItems(this IWadTreeParent parent)
    {
        if (parent.Items is null)
            yield break;

        foreach (var (_, item) in parent.Items)
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

        foreach (var (_, item) in parent.Items)
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

        foreach (var (_, item) in parent.Items)
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
