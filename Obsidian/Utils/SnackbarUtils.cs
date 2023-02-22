using MudBlazor;

namespace Obsidian.Utils;

public static class SnackbarUtils
{
    public static void ShowHardError(ISnackbar snackbar, Exception exception)
    {
        snackbar.Add($"Error: {exception}", Severity.Error, x => x.RequireInteraction = true);
    }

    public static void ShowSoftError(ISnackbar snackbar, Exception exception)
    {
        snackbar.Add(
            $"{exception.Message}",
            Severity.Error,
            x =>
            {
                x.RequireInteraction = false;
                x.VisibleStateDuration = 2000;
            }
        );
    }
}
