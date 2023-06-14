using MudBlazor;
using Serilog;

namespace Obsidian.Utils;

public static class SnackbarUtils {
    public static void ShowHardError(ISnackbar snackbar, Exception exception) {
        Log.Error(exception, "Error");

        snackbar.Add($"Error: {exception}", Severity.Error, x => x.RequireInteraction = true);
    }

    public static void ShowSoftError(ISnackbar snackbar, Exception exception) {
        Log.Error(exception, "Error");

        snackbar.Add(
            $"{exception.Message}",
            Severity.Error,
            x => {
                x.RequireInteraction = false;
                x.VisibleStateDuration = 2000;
            }
        );
    }
}