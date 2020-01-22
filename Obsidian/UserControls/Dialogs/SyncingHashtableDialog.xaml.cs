using Obsidian.Utilities;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Obsidian.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for SyncingHashtableDialog.xaml
    /// </summary>
    public partial class SyncingHashtableDialog : UserControl
    {
        public string Message { get; }

        public SyncingHashtableDialog()
        {
            this.Message = "Syncing hashtables...";

            InitializeComponent();

            this.DataContext = this;
        }

        public async void StartSyncing(object sender, EventArgs e)
        {
            await Sync();
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private async Task Sync()
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
                await SyncGameHashtable();
                await SyncLCUHashtable();

                async Task SyncGameHashtable()
                {
                    if (!File.Exists(Hashtable.GAME_HASHTABLE_FILE) || gameHashesContent.Sha != Config.Get<string>("GameHashtableChecksum"))
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            await webClient.DownloadFileTaskAsync(gameHashesContent.DownloadUrl, Hashtable.GAME_HASHTABLE_FILE);
                        }

                        Config.Set("GameHashtableChecksum", gameHashesContent.Sha);
                    }
                }
                async Task SyncLCUHashtable()
                {
                    if (!File.Exists(Hashtable.LCU_HASHTABLE_FILE) || lcuHashesContent.Sha != Config.Get<string>("LCUHashtableChecksum"))
                    {
                        using (WebClient webClient = new WebClient())
                        {
                            await webClient.DownloadFileTaskAsync(lcuHashesContent.DownloadUrl, Hashtable.LCU_HASHTABLE_FILE);
                        }

                        Config.Set("LCUHashtableChecksum", lcuHashesContent.Sha);
                    }
                }
            }
        }
    }
}
