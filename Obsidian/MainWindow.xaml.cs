using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.Win32;
using Obsidian.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WADFile Wad { get; set; }
        public WADEntry CurrentlySelectedEntry { get; set; }
        public static Dictionary<ulong, string> StringDictionary { get; set; } = new Dictionary<ulong, string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void buttonOpenWadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select the WAD File you want to open";
            dialog.Multiselect = false;
            dialog.Filter = "WAD Files (*.wad;*.wad.client)|*.wad;*.wad.client";

            if (dialog.ShowDialog() == true)
            {
                this.Wad = new WADFile(dialog.FileName);
                StringDictionary = new Dictionary<ulong, string>();
                GenerateWADStrings();

                this.buttonSaveWadFile.IsEnabled = true;
                this.buttonImportHashtable.IsEnabled = true;
                this.buttonExtractHashtable.IsEnabled = true;
                this.buttonAddFile.IsEnabled = true;
                this.butonAddFileRedirection.IsEnabled = true;
                this.CurrentlySelectedEntry = null;
                this.datagridWadEntries.ItemsSource = this.Wad.Entries;
            }
        }

        private void GenerateWADStrings()
        {
            foreach (WADEntry wadEntry in this.Wad.Entries.Where(x => x.Type == EntryType.Compressed))
            {
                byte[] entryData = wadEntry.GetContent(true);
                if (Utilities.GetLeagueFileExtensionType(entryData) == LeagueFileType.BIN)
                {
                    List<string> wadEntryStrings = new List<string>();
                    BINFile bin = new BINFile(new MemoryStream(entryData));
                    foreach (BINFileEntry binEntry in bin.Entries)
                    {
                        foreach (BINFileValue binValue in binEntry.Values.Where(x => x.Type == BINFileValueType.String || x.Value.GetType() == typeof(BINFileValueList)))
                        {
                            if (binValue.Type == BINFileValueType.String)
                            {
                                wadEntryStrings.Add(binValue.Value as string);
                            }
                            else if (
                            binValue.Type == BINFileValueType.DoubleTypeList ||
                            binValue.Type == BINFileValueType.LargeStaticTypeList ||
                            binValue.Type == BINFileValueType.List ||
                            binValue.Type == BINFileValueType.List2 ||
                            binValue.Type == BINFileValueType.SmallStaticTypeList)
                            {
                                wadEntryStrings.AddRange(GetValueStrings(binValue));
                            }
                        }
                    }

                    using (XXHash64 xxHash = XXHash64.Create())
                    {
                        wadEntryStrings.ForEach(x =>
                        {
                            if (x != "")
                            {
                                string loweredName = x.ToLower();
                                ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(loweredName)), 0);
                                if (!StringDictionary.ContainsKey(hash))
                                {
                                    StringDictionary.Add(hash, x);
                                }
                            }
                        });
                    }
                }
            }
        }

        public IEnumerable<string> GetValueStrings(BINFileValue value)
        {
            List<string> strings = new List<string>();
            if (value.Type == BINFileValueType.String)
            {
                strings.Add(value.Value as string);
            }
            else
            {
                BINFileValueList valueList = value.Value as BINFileValueList;
                foreach (BINFileValue binValue in valueList.Entries)
                {
                    if (binValue.Type == BINFileValueType.String)
                    {
                        strings.Add(binValue.Value as string);
                    }
                    else if (
                    binValue.Type == BINFileValueType.DoubleTypeList ||
                    binValue.Type == BINFileValueType.LargeStaticTypeList ||
                    binValue.Type == BINFileValueType.List ||
                    binValue.Type == BINFileValueType.List2 ||
                    binValue.Type == BINFileValueType.SmallStaticTypeList)
                    {
                        foreach (BINFileValue binValue2 in (binValue.Value as BINFileValueList).Entries.Where(x => x.Type == BINFileValueType.String || x.Value.GetType() == typeof(BINFileValueList)))
                        {
                            strings.AddRange(GetValueStrings(binValue2));
                        }
                    }
                }
            }

            return strings.AsEnumerable();
        }

        private void buttonSaveWadFile_Click(object sender, RoutedEventArgs e)
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

        private void buttonImportHashtable_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select the Hashtable files you want to load";
            dialog.Multiselect = true;
            dialog.Filter = "Hashtable Files (*.hashtable)|*.hashtable";

            if (dialog.ShowDialog() == true)
            {
                foreach (string fileName in dialog.FileNames)
                {
                    foreach (string line in File.ReadAllLines(fileName))
                    {
                        ulong hash = 0;
                        string[] lineSplit = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (ulong.TryParse(lineSplit[0], out hash) && !StringDictionary.ContainsKey(hash))
                        {
                            StringDictionary.Add(ulong.Parse(lineSplit[0]), lineSplit[1]);
                        }
                    }
                }

                CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
            }
        }

        private void datagridWadEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
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

                if (StringDictionary.ContainsKey((this.datagridWadEntries.SelectedItem as WADEntry).XXHash))
                {
                    this.textBlockSelectedEntryName.Text = StringDictionary[(this.datagridWadEntries.SelectedItem as WADEntry).XXHash];
                }
                else
                {
                    this.textBlockSelectedEntryName.Text = "";
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

        private void datagridWadEntries_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if ((e.Row.DataContext as WADEntry).Type != EntryType.FileRedirection)
            {
                e.Cancel = true;
            }
        }

        private void buttonAddFile_Click(object sender, RoutedEventArgs e)
        {
            FileAddWindow fileAddWindow = new FileAddWindow(this);
            fileAddWindow.Show();
            this.IsEnabled = false;
        }

        private void buttonAddFileRedirection_Click(object sender, RoutedEventArgs e)
        {
            FileRedirectionAddWindow fileRedirectionAddWindow = new FileRedirectionAddWindow(this);
            fileRedirectionAddWindow.Show();
            this.IsEnabled = false;
        }

        private void buttonRemoveEntry_Click(object sender, RoutedEventArgs e)
        {
            foreach (WADEntry entry in this.datagridWadEntries.SelectedItems.Cast<WADEntry>())
            {
                this.Wad.RemoveEntry(entry.XXHash);
                this.datagridWadEntries.ItemsSource.Cast<WADEntry>().ToList().Remove(entry);
            }
            CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
        }

        private void buttonModifyData_Click(object sender, RoutedEventArgs e)
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

        private void buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                foreach (WADEntry entry in this.datagridWadEntries.SelectedItems.Cast<WADEntry>().Where(x => x.Type != EntryType.FileRedirection))
                {
                    byte[] entryData = entry.GetContent(true);
                    string entryName = "";
                    if (StringDictionary.ContainsKey(entry.XXHash))
                    {
                        entryName = StringDictionary[entry.XXHash];
                        Directory.CreateDirectory(string.Format("{0}//{1}", dialog.SelectedPath, System.IO.Path.GetDirectoryName(entryName)));
                    }
                    else
                    {
                        entryName = BitConverter.ToString(BitConverter.GetBytes(entry.XXHash)).Replace("-", "");
                        entryName += "." + Utilities.GetEntryExtension(Utilities.GetLeagueFileExtensionType(entryData));
                    }

                    File.WriteAllBytes(string.Format("{0}//{1}", dialog.SelectedPath, entryName), entryData);
                }

                MessageBox.Show("Extraction Succesfull!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonExtractHashtable_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Select the path to save your Hashtable File";
            dialog.Filter = "Hashtable File (*.hashtable)|*.hashtable";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == true)
            {
                List<string> lines = new List<string>();
                foreach (KeyValuePair<ulong, string> pair in StringDictionary)
                {
                    lines.Add(pair.Key.ToString() + " " + pair.Value);
                }
                File.WriteAllLines(dialog.FileName, lines.ToArray());

                MessageBox.Show("Writing Succesful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
