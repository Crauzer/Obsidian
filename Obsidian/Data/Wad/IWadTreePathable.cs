﻿namespace Obsidian.Data.Wad;

public interface IWadTreePathable
{
    IWadTreePathable Parent { get; }
    int Depth { get; }

    string Name { get; }
    string Path { get; }
}

public static class IWadTreePathableExtensions
{
    public static string GetPath(this IWadTreePathable pathable) =>
        pathable.Parent switch
        {
            null => pathable.Name,
            _ => string.Join('/', pathable.Parent.Path, pathable.Name)
        };

    public static int GetDepth(this IWadTreePathable pathable) =>
        pathable.Parent switch
        {
            null => 0,
            _ => pathable.Parent.Depth + 1,
        };
}
