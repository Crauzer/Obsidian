using MudBlazor;

namespace Obsidian;

public static class AppTheme
{
    public static readonly MudTheme Theme =
        new()
        {
            Palette = new Palette()
            {
                Primary = Colors.Red.Accent2,
                AppbarBackground = "#712b36",
                DrawerBackground = "#712b36"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = Colors.Red.Accent2,
                AppbarBackground = "#712b36",
                DrawerBackground = "#712b36"
            },
            LayoutProperties = new LayoutProperties() { DefaultBorderRadius = "8px" },
            Typography = new Typography()
            {
                Default = new Default() { FontFamily = new[] { "Fira Sans" } }
            }
        };
}
