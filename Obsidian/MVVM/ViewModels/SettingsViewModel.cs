using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM.Commands;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Obsidian.MVVM.ViewModels
{
    public class SettingsViewModel : PropertyNotifier, ILocalizable
    {
        public Dictionary<string, string> LocalizationMap { get; set; }

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
        public bool EnableDiscordRPC
        {
            get => Config.Get<bool>("EnableDiscordRpc");
            set => Config.Set("EnableDiscordRpc", value);
        }
        public bool CheckForUpdates
        {
            get => Config.Get<bool>("CheckForUpdates");
            set => Config.Set("CheckForUpdates", value);
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
                this.LocalizationMap = Localization.Load();
                NotifyPropertyChanged();
            }
        }

        public List<string> DiscordRPCTimestampModes => Enum.GetNames(typeof(DiscordRpcTimestampMode)).ToList();
        public string SelectedDiscordRPCTimestampMode
        {
            get => Config.Get<DiscordRpcTimestampMode>("DiscordRpcTimestampMode").ToString();
            set
            {
                Config.Set("DiscordRpcTimestampMode", Enum.Parse(typeof(DiscordRpcTimestampMode), value));
                NotifyPropertyChanged();
            }
        }

        public ICommand SelectOpenWadInitialDirectoryCommand => new RelayCommand(SelectOpenWadInitialDirectory);
        public ICommand SelectSaveWadInitialDirectoryCommand => new RelayCommand(SelectSaveWadInitialDirectory);
        public ICommand SelectExtractInitialDirectoryCommand => new RelayCommand(SelectExtractInitialDirectory);

        public SettingsViewModel(Dictionary<string, string> localizationMap)
        {
            this.LocalizationMap = localizationMap;
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
