using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace Obsidian.Windows
{
    /// <summary>
    /// Interaction logic for FileAddWindow.xaml
    /// </summary>
    public partial class FileAddWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        public FileAddWindow(MainWindow window)
        {
            this.MainWindow = window;
            InitializeComponent();
            this.Closed += fileAddWindow_Closed;
        }

        private void fileAddWindow_Closed(object sender, EventArgs e)
        {
            this.MainWindow.IsEnabled = true;
        }

        private void buttonSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textboxFile.Text = openFileDialog.FileName;
                this.buttonAddEntry.IsEnabled = true;
            }
        }

        private void buttonAddEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MainWindow.Wad.AddEntry(this.textboxPath.Text, File.ReadAllBytes(this.textboxFile.Text), this.checkboxCompress.IsChecked.Value);
                CollectionViewSource.GetDefaultView(this.MainWindow.datagridWadEntries.ItemsSource).Refresh();
                this.Close();
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(exception.Message, "Please choose a different Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
