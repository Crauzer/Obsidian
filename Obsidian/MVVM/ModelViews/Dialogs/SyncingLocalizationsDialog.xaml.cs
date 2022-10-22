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
    public partial class SyncingLocalizationsDialog : UserControl
    {
        public string Message { get; }

        public SyncingLocalizationsDialog()
        {
            this.Message = Localization.Get("DialogDownloadingLocalizations");

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
            //Do it in try so we don't crash if there isn't internet connection
            try
            {
                GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("Obsidian"));
                IReadOnlyList<RepositoryContent> files = await gitClient.Repository.Content.GetAllContents("Crauzer", "Obsidian", "Localizations");
                List<string> availableLocalizations = Localization.GetAvailableLocalizations(false);

                foreach(RepositoryContent file in files.Where(x => x.Name.Contains("locale.json")))
                {
                    string fileLocalizationName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.Name));
                    if (!availableLocalizations.Any(x => x == fileLocalizationName))
                    {
                        await using FileStream outputStream = File.Create(Path.Combine(Localization.LOCALIZATION_FOLDER, file.Name));
                        await (await DialogHelper.httpClient.GetStreamAsync(file.DownloadUrl)).CopyToAsync(outputStream);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
