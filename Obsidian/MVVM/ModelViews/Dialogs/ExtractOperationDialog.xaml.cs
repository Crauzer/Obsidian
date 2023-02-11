using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using CommunityToolkit.HighPerformance.Buffers;
using PathIO = System.IO.Path;

namespace Obsidian.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for ExtractOperationDialog.xaml
    /// </summary>
    public partial class ExtractOperationDialog : UserControl, INotifyPropertyChanged
    {
        public string Message
        {
            get => this._message;
            set
            {
                this._message = value;
                NotifyPropertyChanged();
            }
        }
        public double Progress
        {
            get => this._progress;
            set
            {
                this._progress = value;
                NotifyPropertyChanged();
            }
        }
        public double JobCount
        {
            get => this._jobCount;
            set
            {
                this._jobCount = value;
                NotifyPropertyChanged();
            }
        }

        private string _message;
        private double _progress;
        private double _jobCount;

        private readonly string _extractLocation;
        private readonly WadFileViewModel[] _entries;

        public event PropertyChangedEventHandler PropertyChanged;

        public ExtractOperationDialog(string extractLocation, IEnumerable<WadFileViewModel> entries)
        {
            this._entries = entries.ToArray();
            this.JobCount = this._entries.Length;
            this._extractLocation = extractLocation;

            InitializeComponent();

            this.DataContext = this;
        }

        public void StartExtraction(object sender, EventArgs e)
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.WorkerReportsProgress = true;
                worker.DoWork += Extract;
                worker.RunWorkerCompleted += CloseDialog;
                worker.ProgressChanged += ProgressChanged;

                worker.RunWorkerAsync(worker);
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Progress = e.ProgressPercentage;
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private void Extract(object sender, DoWorkEventArgs e)
        {
            double progress = 0;
            HashSet<ulong> packedPaths = new HashSet<ulong>();
            List<string> packedMappingFile = new List<string>();

            GeneratePackedMapping();
            CreateFolders();

            //Write the Packed Mapping file
            if (packedMappingFile.Count != 0)
            {
                File.WriteAllLines(Path.Combine(this._extractLocation, "OBSIDIAN_PACKED_MAPPING.txt"), packedMappingFile);
            }

            //Write the entries
            foreach (WadFileViewModel entry in this._entries)
            {
                string path = Path.Combine(this._extractLocation, entry.Path);
                if (packedPaths.Contains(entry.Entry.PathHash))
                {
                    path = Path.Combine(this._extractLocation, $"{entry.Entry.PathHash:X16}.bin");
                }

                this.Message = entry.Path;

                using MemoryOwner<byte> entryData = entry.ParentWad.LoadChunkDecompressed(entry.Entry);
                using FileStream entryFileStream = File.Create(path);

                entryFileStream.Write(entryData.Span);

                progress++;
                (e.Argument as BackgroundWorker).ReportProgress((int)progress);
            }

            void GeneratePackedMapping()
            {
                this.Message = Localization.Get("DialogExtractWadPackedMappingMessage");

                foreach (WadFileViewModel entry in this._entries)
                {
                    //Check if the entry is a packed bin file
                    if (Regex.IsMatch(entry.Path, Config.Get<string>("PackedBinRegex"), RegexOptions.IgnoreCase))
                    {
                        string line = $"{entry.Entry.PathHash:X16}.bin = {entry.Path}";
                        packedMappingFile.Add(line);
                        packedPaths.Add(entry.Entry.PathHash);
                    }
                }
            }
            void CreateFolders()
            {
                this.Message = Localization.Get("DialogExtractWadCreatingFoldersMessage");

                foreach(WadFileViewModel entry in this._entries)
                {
                    Directory.CreateDirectory(string.Format(@"{0}\{1}", this._extractLocation, PathIO.GetDirectoryName(entry.Path)));
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
