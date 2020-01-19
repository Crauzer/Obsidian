using Fantome.Libraries.League.IO.WAD;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PathIO = System.IO.Path;

namespace Obsidian.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateWadOperationDialog.xaml
    /// </summary>
    public partial class CreateWadOperationDialog : UserControl, INotifyPropertyChanged
    {
        public WadViewModel WadViewModel { get; private set; } = new WadViewModel();
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
        private string _message;

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateWadOperationDialog(string folderLocation)
        {
            this._folderLocation = folderLocation;

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
            string temporaryWad = PathIO.GetTempFileName();
            using (WADFile wad = new WADFile(3, 0))
            {
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

                    bool hasUnknownPath = fileNameWithoutExtension.Length == 16 && fileNameWithoutExtension.All(c => "ABCDEF0123456789".Contains(c));
                    if (hasUnknownPath)
                    {
                        ulong hash = Convert.ToUInt64(fileNameWithoutExtension, 16);
                        wad.AddEntryAutomatic(hash, File.ReadAllBytes(fileLocation), PathIO.GetExtension(fileLocation));
                    }
                    else
                    {
                        wad.AddEntryAutomatic(entryPath, File.ReadAllBytes(fileLocation), PathIO.GetExtension(fileLocation));
                    }
                }

                wad.Write(temporaryWad);
            }

            this.WadViewModel.LoadWad(temporaryWad);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
