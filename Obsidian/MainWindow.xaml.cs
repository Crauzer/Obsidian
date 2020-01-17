using System;
using System.Collections.Generic;
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
using Fantome.Libraries.League.IO.WAD;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using Octokit;

namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Config.Load();
            LoadHashtable();

            InitializeComponent();

            WadViewModel xd = new WadViewModel();
            xd.LoadWad(new WADFile("Aatrox.wad.client"));

            this.WadTree.DataContext = xd;
        }

        private async void LoadHashtable()
        {
            //Check if there is a a new hashtable available
            //Do this in "try" so if there is no internet we don't crash
            try
            {
                SyncHashtable();
            }
            catch
            {
                
            }

            //Load the hashtable after we sync with CDragon
            Hashtable.Load();

            static async void SyncHashtable()
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
                    if(gameHashesContent.Sha != Config.Get<string>("GameHashtableChecksum"))
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

        private async void OnWadOpen(object sender, RoutedEventArgs e)
        {
            
        }

        private void OpenWad()
        {
            try
            {

            }
            catch(Exception exception)
            {

            }
        }
    }
}
