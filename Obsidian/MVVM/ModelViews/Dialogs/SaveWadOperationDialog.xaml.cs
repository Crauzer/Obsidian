using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Obsidian.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for SaveWadOperationDialog.xaml
    /// </summary>
    public partial class SaveWadOperationDialog : UserControl
    {
        public string Message { get; }

        private WadViewModel _wadViewModel;
        private string _wadLocation;

        public SaveWadOperationDialog(string wadLocation, WadViewModel wad)
        {
            this._wadLocation = wadLocation;
            this._wadViewModel = wad;
            this.Message = Localization.Get("DialogSavingWadMessage") + '\n' + wadLocation;

            InitializeComponent();
        }

        public void Save(object sender, EventArgs e)
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += SaveWAD;
                worker.RunWorkerCompleted += CloseDialog;
                worker.WorkerSupportsCancellation = true;

                worker.RunWorkerAsync(worker);
            }
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private void SaveWAD(object sender, DoWorkEventArgs e)
        {
            this._wadViewModel.WAD.Write(this._wadLocation);
            this._wadViewModel.WAD.Dispose();
            this._wadViewModel.WAD = null;
            this._wadViewModel.WADLocation = this._wadLocation;
        }
    }
}
