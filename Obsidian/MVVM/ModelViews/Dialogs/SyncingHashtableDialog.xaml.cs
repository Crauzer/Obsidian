using Obsidian.Utilities;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Obsidian.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for SyncingHashtableDialog.xaml
    /// </summary>
    public partial class SyncingHashtableDialog : UserControl
    {
        public string Message { get; }

        public SyncingHashtableDialog()
        {
            this.Message = Localization.Get("DialogSyncHashesMessage");

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
            //Do this in "try" so if there is no internet or GitHub isn't available we don't crash
            try
            {
                await SyncHashtable();
            }
            catch(Exception)
            {

            }

            //Load the hashtable after we sync with CDragon
            //Check that hashtables are present
            if(File.Exists(Hashtable.GAME_HASHTABLE_FILE))
            {
                Hashtable.Load(Hashtable.GAME_HASHTABLE_FILE);
            }
            if (File.Exists(Hashtable.LCU_HASHTABLE_FILE))
            {
                Hashtable.Load(Hashtable.LCU_HASHTABLE_FILE);
            }

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
                        await using (FileStream outputStream = File.Create(Hashtable.GAME_HASHTABLE_FILE))
                        {
                            await (await DialogHelper.httpClient.GetStreamAsync(gameHashesContent.DownloadUrl)).CopyToAsync(outputStream);
                        }

                        Config.Set("GameHashtableChecksum", gameHashesContent.Sha);
                    }
                }
                async Task SyncLCUHashtable()
                {
                    if (!File.Exists(Hashtable.LCU_HASHTABLE_FILE) || lcuHashesContent.Sha != Config.Get<string>("LCUHashtableChecksum"))
                    {
                        await using (FileStream outputStream = File.Create(Hashtable.LCU_HASHTABLE_FILE))
                        {
                            await (await DialogHelper.httpClient.GetStreamAsync(lcuHashesContent.DownloadUrl)).CopyToAsync(outputStream);
                        }

                        Config.Set("LCUHashtableChecksum", lcuHashesContent.Sha);
                    }
                }
            }
        }
    }
}
