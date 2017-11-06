using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Obsidian.Windows
{
    /// <summary>
    /// Interaction logic for FileRedirectionAddWindow.xaml
    /// </summary>
    public partial class FileRedirectionAddWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        public FileRedirectionAddWindow(MainWindow window)
        {
            this.MainWindow = window;
            InitializeComponent();
            this.Closed += fileRedirectionAddWindow_Closed;
        }

        private void fileRedirectionAddWindow_Closed(object sender, EventArgs e)
        {
            this.MainWindow.IsEnabled = true;
        }

        private void textboxFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.buttonAddFileRedirectionEntry.IsEnabled = this.textboxFile.Text.Length != 0;
        }

        private void buttonAddFileRedirectionEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.MainWindow.Wad.AddEntry(this.textboxFile.Text, this.textboxPath.Text);
                CollectionViewSource.GetDefaultView(this.MainWindow.datagridWadEntries.ItemsSource).Refresh();
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Please choose a different Path", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
