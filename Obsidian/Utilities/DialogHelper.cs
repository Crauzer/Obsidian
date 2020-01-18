using MaterialDesignThemes.Wpf;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.UserControls.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Utilities
{
    public static class DialogHelper
    {
        public static DialogHost MessageDialog { get; set; }
        public static DialogHost OperationDialog { get; set; }
        public static DialogHost RootDialog { get; set; }

        public static async Task<WadViewModel> ShowOpenWadOperartionDialog(string wadLocation)
        {
            OpenWadOperationDialog dialog = new OpenWadOperationDialog(wadLocation);

            await DialogHost.Show(dialog, "OperationDialog", dialog.Load, null);

            return await Task.FromResult(dialog.WadViewModel);
        }
    }
}
