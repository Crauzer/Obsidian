

using Microsoft.WindowsAPICodePack.Dialogs;

namespace Obsidian.Utils;

public static class FileDialogUtils
{

    public static CommonFileDialogFilter CreateWadFilter() => new("Wad Archive", "wad,client,server");

    public static IEnumerable<CommonFileDialogFilter> CreateGltfFilters()
    {
        yield return new("glTF Binary", "glb");
        yield return new("glTF", "gltf");
    }
}
