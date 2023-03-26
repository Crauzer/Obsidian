using CommunityToolkit.Diagnostics;
using LeagueToolkit.Core.Wad;
using Obsidian.Services;
using Obsidian.Shared;
using Serilog;
using PathIO = System.IO.Path;

namespace Obsidian.Data.Wad;

public class WadTreeModel : IWadTreeParent, IDisposable
{
    public HashtableService Hashtable { get; }
    public Config Config { get; }

    public IWadTreePathable Parent => null;
    public int Depth => 0;
    public string Name => string.Empty;
    public string Path => string.Empty;

    public bool UseRegexFilter { get; set; }
    public string Filter { get; set; }

    public WadFilePreviewType CurrentPreviewType { get; set; }

    public HashSet<WadTreeItemModel> Items { get; set; } = new();

    public IEnumerable<WadTreeFileModel> CheckedFiles =>
        this.TraverseFlattenedCheckedItems()
            .Where(x => x is WadTreeFileModel)
            .Select(x => x as WadTreeFileModel);

    public WadTreeFileModel SelectedFile => this.SelectedFiles.FirstOrDefault();

    public IEnumerable<WadTreeFileModel> SelectedFiles =>
        this.TraverseFlattenedItems()
            .Where(x => x.IsSelected && x is WadTreeFileModel)
            .Select(x => x as WadTreeFileModel);

    private Dictionary<string, WadFile> _mountedWadFiles = new();

    public WadFileTextPreview TextPreview { get; set; }

    public bool IsDisposed { get; private set; }

    public WadTreeModel(HashtableService hashtable, Config config, IEnumerable<string> wadFiles)
    {
        Guard.IsNotNull(wadFiles, nameof(wadFiles));

        this.Hashtable = hashtable;
        this.Config = config;

        Rebuild(wadFiles);
    }

    private void Rebuild(IEnumerable<string> wadFiles)
    {
        Log.Information($"Re-building wad tree");

        this.Items.Clear();

        foreach (string wadFilePath in wadFiles)
        {
            CreateTreeForWadFile(
                new(wadFilePath),
                PathIO
                    .GetRelativePath(this.Config.GameDataDirectory, wadFilePath)
                    .Replace(PathIO.DirectorySeparatorChar, '/')
            );
        }

        SortItems();
    }

    private void CreateFileSystemTreeForWadFile(string wadFile)
    {
        string[] pathComponents = wadFile.Split('/');

        this.AddFsFile(pathComponents);
    }

    private void CreateTreeForWadFile(WadFile wad, string wadFilePath)
    {
        foreach (var (_, chunk) in wad.Chunks)
        {
            string path = this.Hashtable.TryGetChunkPath(chunk, out path) switch
            {
                true => path,
                false => this.Hashtable.GuessChunkPath(chunk, wad),
            };

            this.AddWadFile(string.Join('/', wadFilePath, path).Split('/'), wad, chunk);
        }
    }

    private void SortItems()
    {
        Log.Information($"Sorting wad tree");

        this.Items = new(this.Items.OrderBy(x => x));

        foreach (WadTreeItemModel item in this.Items)
        {
            if (item.Type is WadTreeItemType.Directory)
                item.SortItems();
        }
    }

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
        {
            foreach (var (_, wad) in this._mountedWadFiles)
            {
                wad?.Dispose();
            }
        }

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
