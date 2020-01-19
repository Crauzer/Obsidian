using Fantome.Libraries.League.IO.WAD;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Obsidian.UserControls.Dialogs
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
            this.Message = "Saving\n" + wadLocation;

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
        }
    }
}
