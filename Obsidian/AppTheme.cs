using MudBlazor;

namespace Obsidian;

public class AppTheme : MudTheme
{
    public AppTheme() 
    {
        this.Palette = new Palette()
        {
            Primary = Colors.Red.Accent2,
            AppbarBackground = "#712b36",
            DrawerBackground = "#712b36"
        };
        this.PaletteDark = new PaletteDark()
        {
            Primary = Colors.Red.Accent2,
            AppbarBackground = "#712b36",
            DrawerBackground = "#712b36"
        };
        this.LayoutProperties = new LayoutProperties() { DefaultBorderRadius = "8px" };
        this.Typography = new Typography()
        {
            Default = new Default() { FontFamily = new[] { "Fira Sans" } }
        };
    }
}
