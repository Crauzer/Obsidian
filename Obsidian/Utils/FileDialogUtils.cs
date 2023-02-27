using Microsoft.WindowsAPICodePack.Dialogs;

namespace Obsidian.Utils;

public static class FileDialogUtils
{
    public static CommonOpenFileDialog CreateOpenWadDialog(string initialDirectory)
    {
        CommonOpenFileDialog dialog =
            new("Open Wad archives") { Multiselect = true, InitialDirectory = initialDirectory };
        dialog.Filters.Add(CreateWadFilter());

        return dialog;
    }

    public static CommonOpenFileDialog CreateExtractWadDialog(string initialDirectory)
    {
        CommonOpenFileDialog dialog =
            new("Select the extraction directory")
            {
                IsFolderPicker = true,
                InitialDirectory = initialDirectory
            };

        return dialog;
    }

    public static CommonFileDialogFilter CreateWadFilter() =>
        new("Wad Archive", "wad,client,server");

    public static IEnumerable<CommonFileDialogFilter> CreateGltfFilters()
    {
        yield return new("glTF Binary", "glb");
        yield return new("glTF", "gltf");
    }
}
