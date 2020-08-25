using MaterialDesignThemes.Wpf;
using Obsidian.MVVM.ViewModels;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.MVVM.ModelViews.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Obsidian.Utilities
{
    public static class DialogHelper
    {
        public static DialogHost MessageDialog { get; set; }
        public static DialogHost OperationDialog { get; set; }
        public static DialogHost RootDialog { get; set; }

        private static Dictionary<string, string> _localizationMap;

        public static void Initialize(Dictionary<string, string> localizationMap)
        {
            _localizationMap = localizationMap;
        }

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

        public static async Task ShowSettingsDialog()
        {
            SettingsDialog dialog = new SettingsDialog()
            {
                DataContext = new SettingsViewModel(_localizationMap)
            };

            await DialogHost.Show(dialog, "RootDialog");
        }

        public static async Task ShowExtractOperationDialog(string extractLocation, IEnumerable<WadFileViewModel> entries)
        {
            ExtractOperationDialog dialog = new ExtractOperationDialog(extractLocation, entries);

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartExtraction, null);
        }

        public static async Task ShowMessageDialog(string message, bool closeOnClickAway = false)
        {
            MessageDialog dialog = new MessageDialog(message);
            bool defaultCloseOnClickAway = MessageDialog.CloseOnClickAway;

            MessageDialog.CloseOnClickAway = closeOnClickAway;

            await DialogHost.Show(dialog, "MessageDialog");

            MessageDialog.CloseOnClickAway = defaultCloseOnClickAway;
        }

        public static async Task ShowSyncingHashtableDialog()
        {
            SyncingHashtableDialog dialog = new SyncingHashtableDialog();

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartSyncing, null);
        }

        public static async Task ShowSyncingLocalizationsDialog()
        {
            SyncingLocalizationsDialog dialog = new SyncingLocalizationsDialog();

            await DialogHost.Show(dialog, "OperationDialog", dialog.StartSyncing, null);
        }
    }
}
