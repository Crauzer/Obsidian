using MudBlazor;

namespace Obsidian.Utils;

public static class SnackbarUtils
{
    public static void ShowError(ISnackbar snackbar, Exception exception)
    {
        snackbar.Add($"Error: {exception}", Severity.Error, x => x.RequireInteraction = true);
    }
}
