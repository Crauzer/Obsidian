using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.IO.WadFile;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using PathIO = System.IO.Path;

namespace Obsidian.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateWadOperationDialog.xaml
    /// </summary>
    public partial class CreateWadOperationDialog : UserControl, INotifyPropertyChanged
    {
        public WadViewModel WadViewModel { get; private set; }
        public string Message
        {
            get => this._message;
            set
            {
                this._message = value;
                NotifyPropertyChanged();
            }
        }

        private string _folderLocation;
        private string _wadLocation;

        private string _message;

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateWadOperationDialog(string folderLocation, string wadLocation)
        {
            this.WadViewModel = new WadViewModel();
            this._folderLocation = folderLocation;
            this._wadLocation = wadLocation;

            InitializeComponent();

            this.DataContext = this;
        }

        public void StartCreation(object sender, EventArgs e)
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += Create;
                worker.RunWorkerCompleted += CloseDialog;
                worker.WorkerSupportsCancellation = true;

                worker.RunWorkerAsync(worker);
            }
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private void Create(object sender, DoWorkEventArgs e)
        {
            WadBuilder wadBuilder = new WadBuilder();
            var newPathHashes = new Dictionary<ulong, string>();

            foreach (string fileLocation in Directory.EnumerateFiles(this._folderLocation, "*", SearchOption.AllDirectories))
            {
                //Ignore packed mapping 
                if (PathIO.GetFileName(fileLocation) == "OBSIDIAN_PACKED_MAPPING.txt")
                {
                    continue;
                }

                char separator = Pathing.GetPathSeparator(fileLocation);
                string entryPath = fileLocation.Replace(this._folderLocation + separator, "").Replace(separator, '/');
                string fileNameWithoutExtension = PathIO.GetFileNameWithoutExtension(fileLocation);
                this.Message = entryPath;

                WadEntryBuilder entryBuilder = new WadEntryBuilder(WadEntryChecksumType.XXHash3);

                bool hasUnknownPath = fileNameWithoutExtension.Length == 16 && fileNameWithoutExtension.All(c => "ABCDEF0123456789".Contains(c));
                if (hasUnknownPath)
                {
                    ulong hash = Convert.ToUInt64(fileNameWithoutExtension, 16);

                    entryBuilder
                        .WithPathXXHash(hash)
                        .WithFileDataStream(fileLocation);
                }
                else
                {
                    entryBuilder
                        .WithPath(entryPath)
                        .WithFileDataStream(fileLocation);

                    // Add the entry path in case the user is adding new files
                    ulong hash = XXHash.XXH64(Encoding.UTF8.GetBytes(entryPath.ToLower()));
                    if(!newPathHashes.ContainsKey(hash))
                    {
                        newPathHashes.Add(hash, entryPath.ToLower());
                    }
                }

                wadBuilder.WithEntry(entryBuilder);
            }

            Hashtable.Add(newPathHashes);

            wadBuilder.Build(this._wadLocation);
            this.WadViewModel.LoadWad(this._wadLocation);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
