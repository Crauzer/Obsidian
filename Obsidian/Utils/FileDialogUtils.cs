

using Microsoft.WindowsAPICodePack.Dialogs;

namespace Obsidian.Utils;

public static class FileDialogUtils
{

    public static CommonFileDialogFilter CreateWadFilter() => new("Wad Archive", "wad,client,server");
}
