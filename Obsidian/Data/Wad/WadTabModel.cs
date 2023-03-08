using LeagueToolkit.Core.Wad;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Obsidian.Services;
using Obsidian.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Data.Wad;

public class WadTabModel : IDisposable
{
    public HashtableService Hashtable { get; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public WadFile Wad { get; set; }

    public bool UseRegexFilter { get; set; }
    public string Filter { get; set; }

    public WadFilePreviewType CurrentPreviewType { get; set; }

    public HashSet<WadItemModel> Items { get; set; } = new();
    public IEnumerable<WadFileModel> CheckedFiles =>
        TraverseFlattenedCheckedItems()
            .Where(x => x is WadFileModel)
            .Select(x => x as WadFileModel);

    public WadFileModel SelectedFile => this.SelectedFiles.FirstOrDefault();

    public IEnumerable<WadFileModel> SelectedFiles =>
        TraverseFlattenedItems()
            .Where(x => x.IsSelected && x is WadFileModel)
            .Select(x => x as WadFileModel);

    public WadFileTextPreview TextPreview { get; set; }

    public bool IsDisposed { get; private set; }

    public WadTabModel(string name, WadFile wad, HashtableService hashtable)
    {
        this.Name = name;
        this.Wad = wad;
        this.Hashtable = hashtable;

        InitializeTree();
    }

    private void InitializeTree()
    {
        foreach (var (chunkPathHash, chunk) in this.Wad.Chunks)
        {
            string path = this.Hashtable.GetChunkPath(chunk);
            string[] pathComponents = path.Split('/');

            if (pathComponents.Length is 1)
                CreateRootWadFile(path, chunk);
            else
                CreateNestedWadFile(pathComponents, chunk);
        }

        SortItems();
    }

    private void CreateRootWadFile(string path, WadChunk chunk)
    {
        this.Items.Add(new WadFileModel(null, path, chunk));
    }

    private void CreateNestedWadFile(IEnumerable<string> pathComponents, WadChunk chunk)
    {
        string folderName = pathComponents.First();
        WadFolderModel folder = (WadFolderModel)
            this.Items.FirstOrDefault(x => x is WadFolderModel && x.Name == folderName);

        if (folder is null)
        {
            folder = new(null, folderName);
            this.Items.Add(folder);
        }

        folder.AddFile(pathComponents.Skip(1), chunk);
    }

    private void SortItems()
    {
        this.Items = new(this.Items.OrderBy(x => x));

        foreach (WadItemModel item in this.Items)
        {
            if (item is WadFolderModel folder)
                folder.SortItems();
        }
    }

    public List<WadItemModel> GetFlattenedItems() => TraverseFlattenedVisibleItems().ToList();

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

    public IEnumerable<WadItemModel> TraverseFlattenedVisibleItems()
    {
        foreach (WadItemModel item in this.Items)
        {
            if (!string.IsNullOrEmpty(this.Filter))
            {
                // If the current item is a file we check if it matches the filter
                if (
                    item is WadFileModel
                    && WadItemModel.DoesMatchFilter(item, this.Filter, this.UseRegexFilter)
                )
                {
                    yield return item;
                    continue;
                }

                // If the current item is a folder we get filtered items and if there are none we skip
                IReadOnlyList<WadItemModel> filteredItems = item.TraverseFlattenedVisibleItems(
                        this.Filter,
                        this.UseRegexFilter
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

    public string GetViewportContainerId() => $"{this.Id}_viewport_container";

    public string GetCodeEditorId() => $"{this.Id}_codeEditor";

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.IsDisposed)
            return;

        if (disposing)
            this.Wad?.Dispose();

        this.IsDisposed = true;
    }
}

public enum WadFilePreviewType
{
    None,
    Image,
    Viewport,
    Text
}
