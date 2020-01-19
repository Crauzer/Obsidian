﻿using Fantome.Libraries.League.IO.WAD;
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
    /// Interaction logic for OpenWadOperationDialog.xaml
    /// </summary>
    public partial class OpenWadOperationDialog : UserControl
    {
        public string Message { get; }
        public WadViewModel WadViewModel { get; private set; } = new WadViewModel();

        private string _wadLocation;

        public OpenWadOperationDialog(string wadLocation)
        {
            this._wadLocation = wadLocation;
            this.Message = "Opening\n" + wadLocation;

            InitializeComponent();
        }

        public void Load(object sender, EventArgs e)
        {
            using (BackgroundWorker worker = new BackgroundWorker())
            {
                worker.DoWork += OpenWAD;
                worker.RunWorkerCompleted += CloseDialog;
                worker.WorkerSupportsCancellation = true;

                worker.RunWorkerAsync(worker);
            }
        }

        private void CloseDialog(object sender, RunWorkerCompletedEventArgs e)
        {
            DialogHelper.OperationDialog.IsOpen = false;
        }

        private void OpenWAD(object sender, DoWorkEventArgs e)
        {
            this.WadViewModel.LoadWad(this._wadLocation);
        }
    }
}