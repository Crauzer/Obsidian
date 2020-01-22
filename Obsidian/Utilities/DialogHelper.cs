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
        public static async Task ShowSaveWadOperationDialog(string wadLocation, WadViewModel wad)
        {
            SaveWadOperationDialog dialog = new SaveWadOperationDialog(wadLocation, wad);

            await DialogHost.Show(dialog, "OperationDialog", dialog.Save, null);
        }
        public static async Task<WadViewModel> ShowCreateWADOperationDialog(string folderLocation)
        {
            CreateWadOperationDialog dialog = new CreateWadOperationDialog(folderLocation);

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartCreation, null);

            return await Task.FromResult(dialog.WadViewModel);
        }

        public static async Task ShowExtractOperationDialog(string extractLocation, IEnumerable<WadFileViewModel> entries)
        {
            ExtractOperationDialog dialog = new ExtractOperationDialog(extractLocation, entries);

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartExtraction, null);
        }

        public static async Task ShowMessageDialog(string message)
        {
            MessageDialog dialog = new MessageDialog(message);

            await DialogHost.Show(dialog, "MessageDialog");
        }

        public static async Task ShowSyncingHashtableDialog()
        {
            SyncingHashtableDialog dialog = new SyncingHashtableDialog();

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartSyncing, null);
        }
    }
}
