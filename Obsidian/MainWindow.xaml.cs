using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using Octokit;
using PathIO = System.IO.Path;


namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public WadViewModel WAD
        {
            get => this._wad;
            private set
            {
                this._wad = value;
                this.IsWadOpened = true;
                NotifyPropertyChanged();
            }
        }
        public bool IsWadOpened
        {
            get => this._isWadOpened;
            set
            {
                this._isWadOpened = value;
                NotifyPropertyChanged();
            }
        }

        private WadViewModel _wad = new WadViewModel();
        private bool _isWadOpened;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Config.Load();
            LoadHashtable();

            InitializeComponent();
            BindMVVM();
        }

        private async void LoadHashtable()
        {
            //Check if there is a a new hashtable available
            //Do this in "try" so if there is no internet we don't crash
            try
            {
                await SyncHashtable();
            }
            catch
            {

            }

            //Load the hashtable after we sync with CDragon
            Hashtable.Load();

            static async Task SyncHashtable()
            {
                GitHubClient githubClient = new GitHubClient(new ProductHeaderValue("Obsidian"));
                IReadOnlyList<RepositoryContent> content = await githubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox");
                RepositoryContent gameHashesContent = content.FirstOrDefault(x => x.Name == "hashes.game.txt");
                RepositoryContent lcuHashesContent = content.FirstOrDefault(x => x.Name == "hashes.lcu.txt");

                //Compare GitHub checksums to ours, if they're different then we download new hashtables
                SyncGameHashtable();
                SyncLCUHashtable();

                void SyncGameHashtable()
                {
                    if (gameHashesContent.Sha != Config.Get<string>("GameHashtableChecksum"))
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.DownloadFile(gameHashesContent.DownloadUrl, Hashtable.GAME_HASHTABLE_FILE);
                        }

                        Config.Set("GameHashtableChecksum", gameHashesContent.Sha);
                    }
                }
                void SyncLCUHashtable()
                {
                    if (lcuHashesContent.Sha != Config.Get<string>("LCUHashtableChecksum"))
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.DownloadFile(lcuHashesContent.DownloadUrl, Hashtable.LCU_HASHTABLE_FILE);
                        }

                        Config.Set("LCUHashtableChecksum", lcuHashesContent.Sha);
                    }
                }
            }
        }
        private void BindMVVM()
        {
            this.DataContext = this;

            DialogHelper.MessageDialog = this.MessageDialog;
            DialogHelper.OperationDialog = this.OperationDialog;
            DialogHelper.RootDialog = this.RootDialog;
        }

        private void OnWadOpen(object sender, RoutedEventArgs e)
        {
            OpenWad();
        }
        private void OnWadSave(object sender, RoutedEventArgs e)
        {
            SaveWad();
        }

        private void OnExtractAll(object sender, RoutedEventArgs e)
        {
            ExtractAll();
        }
        private void OnExtractSelected(object sender, RoutedEventArgs e)
        {
            ExtractSelected();
        }

        private void OnFolderAddFiles(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = true;
                
                if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    WadFolderViewModel wadFolder = (sender as FrameworkElement).DataContext as WadFolderViewModel;

                    foreach(string fileLocation in dialog.FileNames)
                    {
                        wadFolder.AddFile(fileLocation);
                    }
                }
            }
        }
        private void OnFolderAddFolder(object sender, RoutedEventArgs e)
        {

        }
        private void OnFolderRemove(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenWad()
        {
            try
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Filters.Add(new CommonFileDialogFilter("WAD Files", "*.wad;*.client;*.mobile"));

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        this.WAD = await DialogHelper.ShowOpenWadOperartionDialog(dialog.FileName);
                    }
                }
            }
            catch (Exception exception)
            {

            }
        }
        private async void SaveWad()
        {
            using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
            {
                dialog.AlwaysAppendDefaultExtension = true;
                dialog.DefaultExtension = ".client";
                dialog.Filters.Add(new CommonFileDialogFilter("wad.client File", "*.client"));
                dialog.Filters.Add(new CommonFileDialogFilter("wad File", "*.wad"));

                if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string wadLocation = dialog.FileName;

                    //We need to change the extension because the dialog is stupid 
                    //and can't handle extensions with multiple dots
                    if(PathIO.GetExtension(dialog.FileName) == ".client")
                    {
                        wadLocation = PathIO.ChangeExtension(dialog.FileName, ".wad.client");
                    }

                    await DialogHelper.ShowSaveWadOperationDialog(wadLocation, this.WAD);
                }
            }
        }

        private async void ExtractAll()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetAllEntries());
                }
            }
        }
        private async void ExtractSelected()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetSelectedEntries());
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
