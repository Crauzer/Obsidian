namespace Obsidian.Data.Wad;

public static class WadTreeUtils
{
    public static string ComposeWadTreeItemPathComponents(IEnumerable<string> pathComponents) =>
        string.Join('/', pathComponents);
}
