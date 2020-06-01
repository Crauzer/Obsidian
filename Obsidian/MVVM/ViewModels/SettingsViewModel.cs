using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM.Commands;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Obsidian.MVVM.ViewModels
{
    public class SettingsViewModel : PropertyNotifier
    {
        public MainWindow MainWindow { get; }

        public bool GenerateHashesFromBIN
        {
            get => Config.Get<bool>("GenerateHashesFromBIN");
            set => Config.Set("GenerateHashesFromBIN", value);
        }
        public bool SyncHashes
        {
            get => Config.Get<bool>("SyncHashes");
            set => Config.Set("SyncHashes", value);
        }
        public string OpenWadInitialDirectory
        {
            get => Config.Get<string>("OpenWadInitialDirectory");
            set
            {
                Config.Set("OpenWadInitialDirectory", value);
                NotifyPropertyChanged();
            }
        }
        public string SaveWadInitialDirectory
        {
            get => Config.Get<string>("SaveWadInitialDirectory");
            set
            {
                Config.Set("SaveWadInitialDirectory", value);
                NotifyPropertyChanged();
            }
        }
        public string ExtractInitialDirectory
        {
            get => Config.Get<string>("ExtractInitialDirectory");
            set
            {
                Config.Set("ExtractInitialDirectory", value);
                NotifyPropertyChanged();
            }
        }
        public List<string> Locales => Localization.GetAvailableLocalizations(true);
        public string SelectedLocale
        {
            get => Config.Get<string>("Localization");
            set
            {
                Config.Set("Localization", value);
                this.MainWindow.LocalizationMap = Localization.Load();
                NotifyPropertyChanged();
            }
        }

        public ICommand SelectOpenWadInitialDirectoryCommand => new RelayCommand(SelectOpenWadInitialDirectory);
        public ICommand SelectSaveWadInitialDirectoryCommand => new RelayCommand(SelectSaveWadInitialDirectory);
        public ICommand SelectExtractInitialDirectoryCommand => new RelayCommand(SelectExtractInitialDirectory);

        public SettingsViewModel(MainWindow mainWindow)
        {
            this.MainWindow = mainWindow;
        }

        private void SelectOpenWadInitialDirectory(object o)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.OpenWadInitialDirectory = dialog.FileName;
                }
            }
        }
        private void SelectSaveWadInitialDirectory(object o)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.SaveWadInitialDirectory = dialog.FileName;
                }
            }
        }
        private void SelectExtractInitialDirectory(object o)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.ExtractInitialDirectory = dialog.FileName;
                }
            }
        }
    }
}
