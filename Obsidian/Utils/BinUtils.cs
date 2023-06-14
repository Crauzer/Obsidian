using LeagueToolkit.Meta;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Obsidian.Utils;

public static partial class BinUtils {
    public static MetaEnvironment CreateMetaEnvironment() =>
        MetaEnvironment.Create(
            Assembly.Load("LeagueToolkit.Meta.Classes").ExportedTypes.Where(x => x.IsClass)
        );

    public static bool IsSkinPackage(string path) => SkinPackageRegex().IsMatch(path);

    public static bool IsSkinAnimations(string path) => SkinAnimationsRegex().IsMatch(path);

    [GeneratedRegex(
        "data\\/characters\\/\\w+\\/skins\\/skin\\d+\\.bin",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex SkinPackageRegex();

    [GeneratedRegex(
        "data\\/characters\\/\\w+\\/animations\\/skin\\d+\\.bin",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex SkinAnimationsRegex();
}