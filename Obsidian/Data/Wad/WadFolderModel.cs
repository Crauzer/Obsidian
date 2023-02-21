using LeagueToolkit.Core.Wad;
using System.Diagnostics;


namespace Obsidian.Data.Wad;

[DebuggerDisplay("{Name}")]
public class WadFolderModel : WadItemModel
{
    public WadFolderModel(WadFolderModel parent, string name)
    {
        this.Parent = parent;
        this.Name = name;
        this.Items = new();
    }

    public void AddFile(IEnumerable<string> pathComponents, WadChunk chunk)
    {
        // File belongs to this folder
        if (pathComponents.Count() is 1)
        {
            this.Items.Add(new WadFileModel(this, pathComponents.First(), chunk));
            return;
        }

        string folderName = pathComponents.First();
        WadFolderModel folder = (WadFolderModel)
            this.Items.FirstOrDefault(x => x is WadFolderModel && x.Name == folderName);

        if (folder is null)
        {
            folder = new(this, folderName);
            this.Items.Add(folder);
        }

        folder.AddFile(pathComponents.Skip(1), chunk);
    }
}
