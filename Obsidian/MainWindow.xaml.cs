using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.Win32;
using Obsidian.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;

namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WADFile Wad { get; set; }
        public WADEntry CurrentlySelectedEntry { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void ButtonOpenWadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select the WAD File you want to open";
            dialog.Multiselect = false;
            dialog.Filter = "WAD Files (*.wad;*.wad.client)|*.wad;*.wad.client";

            if (dialog.ShowDialog() == true)
            {
                this.Wad = new WADFile(dialog.FileName);
                this.buttonSaveWadFile.IsEnabled = true;
                this.buttonAddFile.IsEnabled = true;
                this.butonAddFileRedirection.IsEnabled = true;
                this.CurrentlySelectedEntry = null;
                this.datagridWadEntries.ItemsSource = this.Wad.Entries;
            }
        }

        private void ButtonSaveWadFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Select the path to save your WAD File";
            dialog.Filter = "WAD File (*.wad)|*.wad|WAD Client file (*.wad.client)|*.wad.client";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                this.Wad.Write(filePath);
                MessageBox.Show("Writing Succesful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DatagridWadEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (this.datagridWadEntries.SelectedItem != null && this.datagridWadEntries.SelectedItem is WADEntry)
            {
                this.buttonRemoveEntry.IsEnabled = true;
                if ((this.datagridWadEntries.SelectedItem as WADEntry).Type != EntryType.FileRedirection)
                {
                    this.CurrentlySelectedEntry = this.datagridWadEntries.SelectedItem as WADEntry;
                    this.buttonModifyData.IsEnabled = true;
                }
                else
                {
                    this.CurrentlySelectedEntry = null;
                    this.buttonModifyData.IsEnabled = false;
                }
            }

            if (this.datagridWadEntries.SelectedItems != null && this.datagridWadEntries.SelectedItems.Cast<WADEntry>().ToList().Exists(x => x.Type != EntryType.FileRedirection))
            {
                this.buttonExtract.IsEnabled = true;
            }
            else
            {
                this.buttonExtract.IsEnabled = false;
            }
        }

        private void DatagridWadEntries_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if ((e.Row.DataContext as WADEntry).Type != EntryType.FileRedirection)
            {
                e.Cancel = true;
            }
        }

        private void ButtonAddFile_Click(object sender, RoutedEventArgs e)
        {
            FileAddWindow fileAddWindow = new FileAddWindow(this);
            fileAddWindow.Show();
            this.IsEnabled = false;
        }

        private void ButonAddFileRedirection_Click(object sender, RoutedEventArgs e)
        {
            FileRedirectionAddWindow fileRedirectionAddWindow = new FileRedirectionAddWindow(this);
            fileRedirectionAddWindow.Show();
            this.IsEnabled = false;
        }

        private void ButtonRemoveEntry_Click(object sender, RoutedEventArgs e)
        {
            foreach (WADEntry entry in this.datagridWadEntries.SelectedItems.Cast<WADEntry>())
            {
                this.Wad.RemoveEntry(entry.XXHash);
                this.datagridWadEntries.ItemsSource.Cast<WADEntry>().ToList().Remove(entry);
            }
            CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
        }

        private void ButtonModifyData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Select the File by which you want to replace the current one";

            if (dialog.ShowDialog() == true)
            {
                this.CurrentlySelectedEntry.EditData(File.ReadAllBytes(dialog.FileName));
                CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
                MessageBox.Show("Entry Modified Succesfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ButtonExtract_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                foreach (WADEntry entry in this.datagridWadEntries.SelectedItems.Cast<WADEntry>().Where(x => x.Type != EntryType.FileRedirection))
                {
                    byte[] dataToWrite = entry.GetContent(true);

                    File.WriteAllBytes(string.Format("{0}//{1}.{2}",
                        dialog.SelectedPath,
                        Utilities.ByteArrayToHex(BitConverter.GetBytes(entry.XXHash), true),
                        Utilities.GetEntryExtension(Utilities.GetLeagueFileExtensionType(dataToWrite))),
                        dataToWrite);
                }

                MessageBox.Show("Extraction Succesfull!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
