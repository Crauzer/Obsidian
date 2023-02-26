using System.Text.RegularExpressions;

namespace Obsidian.Utils;

public static partial class BinUtils
{
    public static bool IsSkinPackage(string path) =>
        SkinPackageRegex().IsMatch(path);

    [GeneratedRegex("data\\/characters\\/\\w+\\/skins\\/skin\\d+\\.bin", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SkinPackageRegex();
}
