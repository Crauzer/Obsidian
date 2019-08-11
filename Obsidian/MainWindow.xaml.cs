using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.WAD;
using log4net;
using log4net.Core;
using Microsoft.Win32;
using Obsidian.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Fantome.Libraries.League.Helpers.Cryptography;
using System.Text;
using MaterialDesignThemes.Wpf;
using Fantome.Libraries.League.IO.BIN;
using System.Text.RegularExpressions;

namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, object> Config { get; }
        private static readonly ILog Logger = LogManager.GetLogger("MainWindow");
        public WADFile Wad { get; set; }
        public WADEntry CurrentlySelectedEntry { get; set; }
        public static Dictionary<ulong, string> StringDictionary { get; set; } = new Dictionary<ulong, string>();

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (File.Exists("config.json"))
            {
                this.Config = ConfigUtilities.ReadConfig();
            }
            else
            {
                this.Config = ConfigUtilities.DefaultConfig;
                ConfigUtilities.WriteDefaultConfig();
            }

            Logging.InitializeLogger((string)this.Config["LoggingPattern"], this.Config["LogLevel"] as Level);
            Logger.Info("Initialized Logger");

            InitializeComponent();
            Logger.Info("Initialized Window");
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logging.LogFatal(Logger, "An unhandled exception was thrown, the program will now terminate", (Exception)e.ExceptionObject);
        }

        private void datagridWadEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (this.datagridWadEntries.SelectedItem is WADEntry entry)
            {
                this.menuRemove.IsEnabled = true;
                if (entry.Type != EntryType.FileRedirection)
                {
                    this.CurrentlySelectedEntry = entry;
                    this.menuModifyData.IsEnabled = true;
                }
                else
                {
                    this.CurrentlySelectedEntry = null;
                    this.menuModifyData.IsEnabled = false;
                }
            }

            this.menuExportSelected.IsEnabled = this.datagridWadEntries.SelectedItems.Cast<WADEntry>().ToList().Exists(x => x.Type != EntryType.FileRedirection);
        }

        private void datagridWadEntries_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (((WADEntry)e.Row.DataContext).Type != EntryType.FileRedirection)
            {
                e.Cancel = true;
            }
        }

        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select the WAD File you want to open",
                Multiselect = false,
                Filter = "WAD Files (*.wad;*.wad.client)|*.wad;*.wad.client"
            };

            if (dialog.ShowDialog() == true)
            {
                OpenWADFile(dialog.FileName);
            }
        }

        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Select the path to save your WAD File",
                Filter = "WAD File (*.wad)|*.wad|WAD Client file (*.wad.client)|*.wad.client",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    //this.Wad.Entries.OrderBy(entry => entry.XXHash);
                    this.Wad.Write(dialog.FileName, (byte)(long)this.Config["WadSaveMajorVersion"], (byte)(long)this.Config["WadSaveMinorVersion"]);
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Could not write a WAD File to: " + dialog.FileName, excp);
                    return;
                }

                MessageBox.Show("Writing Succesful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.Info("Successfully written a WAD File to: " + dialog.FileName);
            }
        }

        private void menuImportHashtable_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select the Hashtable files you want to load",
                Multiselect = true,
                Filter = "Text Files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    foreach (string fileName in dialog.FileNames)
                    {
                        foreach (string line in File.ReadAllLines(fileName))
                        {
                            using (XXHash64 xxHash = XXHash64.Create())
                            {
                                ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(line.ToLower())), 0); ;
                                if (!StringDictionary.ContainsKey(hash))
                                {
                                    StringDictionary.Add(hash, line);
                                }
                            }
                        }

                        Logger.Info("Imported Hashtable from: " + fileName);
                    }
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Failed to Import Hashtable", excp);
                    return;
                }

                CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
            }
        }

        private void menuExportHashtable_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Select the path to save the currently generated hashtable",
                Filter = "Text File (*.txt)|*.txt",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string[] lines = new string[StringDictionary.Count];
                    for(int i = 0; i < StringDictionary.Count; i++)
                    {
                        lines[i] = StringDictionary.ElementAt(i).Value;
                    }

                    File.WriteAllLines(dialog.FileName, lines.ToArray());
                }
                catch (Exception exception)
                {
                    Logging.LogException(Logger, "Failed to Extract the current Hashtable", exception);
                    return;
                }

                MessageBox.Show("Writing Succesful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void menuExportAll_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ExtractWADEntries(dialog.SelectedPath, this.datagridWadEntries.Items.Cast<WADEntry>().Where(x => x.Type != EntryType.FileRedirection).ToList());
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Extraction of the currently opened WAD File failed", excp);
                }
            }
        }

        private void menuExportSelected_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ExtractWADEntries(dialog.SelectedPath, this.datagridWadEntries.SelectedItems.Cast<WADEntry>().Where(x => x.Type != EntryType.FileRedirection).ToList());
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Extraction of the currently opened WAD File failed", excp);
                }
            }
        }

        private void menuAddFileRedirection_Click(object sender, RoutedEventArgs e)
        {
            //Don't need these for file redirection
            this.buttonAddFileOpen.Visibility = Visibility.Collapsed;
            this.checkboxAddFileCompressed.Visibility = Visibility.Collapsed;

            //File redirection needs other hints
            HintAssist.SetHint(this.textboxAddFileFilePath, "Path");
            HintAssist.SetHint(this.textboxAddFilePath, "File Redirection");
        }

        private void menuRemove_Click(object sender, RoutedEventArgs e)
        {
            foreach (WADEntry entry in this.datagridWadEntries.SelectedItems.Cast<WADEntry>())
            {
                this.Wad.RemoveEntry(entry.XXHash);
                this.datagridWadEntries.ItemsSource.Cast<WADEntry>().ToList().Remove(entry);
            }
            CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
        }

        private void menuModifyData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select the File by which you want to replace the current one"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    this.CurrentlySelectedEntry.EditData(File.ReadAllBytes(dialog.FileName));
                    CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
                }
                catch (Exception excp)
                {
                    string entryName = BitConverter.ToString(BitConverter.GetBytes(this.CurrentlySelectedEntry.XXHash)).Replace("-", "");
                    if (StringDictionary.ContainsKey(this.CurrentlySelectedEntry.XXHash))
                    {
                        entryName = StringDictionary[this.CurrentlySelectedEntry.XXHash];
                    }

                    Logging.LogException(Logger, "Failed to modify the data of Entry: " + entryName, excp);
                    return;
                }

                Logger.Info("Modified Data of Entry: " + BitConverter.ToString(BitConverter.GetBytes(this.CurrentlySelectedEntry.XXHash)).Replace("-", ""));
                MessageBox.Show("Entry Modified Succesfully!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void textBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.datagridWadEntries.Items.Filter = new Predicate<object>(entry =>
            {
                string finalName = "";
                if (StringDictionary.ContainsKey((entry as WADEntry).XXHash))
                {
                    finalName = StringDictionary[(entry as WADEntry).XXHash];
                }
                else
                {
                    finalName = BitConverter.ToString(BitConverter.GetBytes((entry as WADEntry).XXHash)).Replace("-", "");
                }

                return finalName.ToLower().Contains(this.textBoxFilter.Text.ToLower());
            });


        }

        private void buttonAddFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                this.textboxAddFileFilePath.Text = openFileDialog.FileName;
                this.buttonAddFileAdd.IsEnabled = true;
            }
        }

        private void buttonAddFileAdd_Click(object sender, RoutedEventArgs e)
        {
            //Actual File
            if (this.buttonAddFileOpen.Visibility == Visibility.Visible)
            {
                try
                {
                    AddFile(this.textboxAddFilePath.Text, File.ReadAllBytes(this.textboxAddFileFilePath.Text), this.checkboxAddFileCompressed.IsChecked.Value, true);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Please choose a different Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            // File Redirection
            else
            {
                try
                {
                    this.Wad.AddEntry(this.textboxAddFileFilePath.Text, this.textboxAddFilePath.Text);

                    using (XXHash64 xxHash = XXHash64.Create())
                    {
                        ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(this.textboxAddFileFilePath.Text.ToLower())), 0);
                        if(!StringDictionary.ContainsKey(hash))
                        {
                            StringDictionary.Add(hash, this.textboxAddFileFilePath.Text);
                        }
                    }

                    CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Please choose a different Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void menuAddFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo directory = new DirectoryInfo(dialog.SelectedPath);

                using (XXHash64 xxHash = XXHash64.Create())
                {
                    foreach (FileInfo fileInfo in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        string path = fileInfo.FullName.Substring(directory.FullName.Length + 1);
                        ulong hashedPath = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower())), 0);

                        if (!StringDictionary.ContainsKey(hashedPath))
                        {
                            StringDictionary.Add(hashedPath, path);
                        }

                        this.Wad.AddEntry(hashedPath, File.ReadAllBytes(fileInfo.FullName), true);
                    }
                }

                Logger.Info("Added files from directory: " + dialog.SelectedPath);
            }
        }

        private void dialogHost_DialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            //Default Values
            this.textboxAddFileFilePath.Text = "";
            this.textboxAddFilePath.Text = "";
            this.checkboxAddFileCompressed.IsChecked = true;

            //Default Hints
            HintAssist.SetHint(this.textboxAddFileFilePath, "File Path");
            HintAssist.SetHint(this.textboxAddFilePath, "Path");

            //Default Visibility
            this.checkboxAddFileCompressed.Visibility = Visibility.Visible;
            this.buttonAddFileOpen.Visibility = Visibility.Visible;
        }

        private void menuCreateEmpty_Click(object sender, RoutedEventArgs e)
        {
            this.Wad?.Dispose();
            this.Wad = new WADFile((byte)(long)this.Config["WadSaveMajorVersion"], (byte)(long)this.Config["WadSaveMinorVersion"]);

            StringDictionary = new Dictionary<ulong, string>();

            if ((bool)this.Config["GenerateWadDictionary"])
            {
                try
                {
                    WADHashGenerator.GenerateWADStrings(Logger, this.Wad, StringDictionary);
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Failed to Generate WAD String Dictionary", excp);
                }
            }

            this.menuSave.IsEnabled = true;
            this.menuImportHashtable.IsEnabled = true;
            this.menuExportHashtable.IsEnabled = true;
            this.menuExportAll.IsEnabled = true;
            this.menuAddFile.IsEnabled = true;
            this.menuAddFileRedirection.IsEnabled = true;
            this.menuAddFolder.IsEnabled = true;
            this.CurrentlySelectedEntry = null;
            this.datagridWadEntries.ItemsSource = this.Wad.Entries;

            Logger.Info("Created empty WAD File");
        }

        private void menuCreateFromDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Wad?.Dispose();
                this.Wad = new WADFile((byte)(long)this.Config["WadSaveMajorVersion"], (byte)(long)this.Config["WadSaveMinorVersion"]);
                StringDictionary = new Dictionary<ulong, string>();

                DirectoryInfo directory = new DirectoryInfo(dialog.SelectedPath);
                foreach (FileInfo fileInfo in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string path = fileInfo.FullName.Substring(directory.FullName.Length + 1).Replace(@"\", "/");
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    bool hasUnknownPath = fileNameWithoutExtension.All(c => "ABCDEF0123456789".Contains(c)) && fileNameWithoutExtension.Length <= 16;

                    if (hasUnknownPath)
                    {
                        ulong hashedPath = HexStringToUInt64(fileNameWithoutExtension);
                        AddFile(hashedPath, File.ReadAllBytes(fileInfo.FullName), true, false);
                    }
                    else
                    {
                        AddFile(path, File.ReadAllBytes(fileInfo.FullName), true, false);
                    }
                }

                if ((bool)this.Config["GenerateWadDictionary"])
                {
                    try
                    {
                        WADHashGenerator.GenerateWADStrings(Logger, this.Wad, StringDictionary);
                    }
                    catch (Exception excp)
                    {
                        Logging.LogException(Logger, "Failed to Generate WAD String Dictionary", excp);
                    }
                }

                this.menuSave.IsEnabled = true;
                this.menuImportHashtable.IsEnabled = true;
                this.menuExportHashtable.IsEnabled = true;
                this.menuExportAll.IsEnabled = true;
                this.menuAddFile.IsEnabled = true;
                this.menuAddFileRedirection.IsEnabled = true;
                this.menuAddFolder.IsEnabled = true;
                this.CurrentlySelectedEntry = null;
                this.datagridWadEntries.ItemsSource = this.Wad.Entries;

                Logger.Info("Created WAD file from directory: " + dialog.SelectedPath);
                CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
            }
        }

        private void datagridWadEntries_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropPath = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (dropPath.Length != 1)
                {
                    MessageBox.Show("You cannot open more than one file", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string extension = Path.GetExtension(dropPath[0]);
                if (extension != ".client" && extension != ".wad")
                {
                    MessageBox.Show("This is not a valid WAD file", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                OpenWADFile(dropPath[0]);
            }
        }

        private void ExtractWADEntries(string selectedPath, List<WADEntry> entries)
        {
            this.progressBarWadExtraction.Maximum = entries.Count;
            this.IsEnabled = false;

            BackgroundWorker wadExtractor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            wadExtractor.ProgressChanged += (sender, args) =>
            {
                this.progressBarWadExtraction.Value = args.ProgressPercentage;
            };

            wadExtractor.DoWork += (sender, e) =>
            {
                Dictionary<string, byte[]> fileEntries = new Dictionary<string, byte[]>();
                double progress = 0;

                foreach (WADEntry entry in entries)
                {
                    byte[] entryData = entry.GetContent(true);
                    string entryName;
                    if (StringDictionary.ContainsKey(entry.XXHash))
                    {
                        entryName = StringDictionary[entry.XXHash];
                        int lastSeparatorPosition = entryName.LastIndexOf('/');
                        Directory.CreateDirectory(Path.Combine(selectedPath, entryName.Substring(0, lastSeparatorPosition + 1)));
                    }
                    else
                    {
                        entryName = BitConverter.ToString(BitConverter.GetBytes(entry.XXHash)).Replace("-", "");
                        entryName += "." + Utilities.GetEntryExtension(Utilities.GetLeagueFileExtensionType(entryData));
                    }

                    fileEntries.Add(entryName, entryData);
                    progress += 0.5;
                    wadExtractor.ReportProgress((int)progress);
                }

                if ((bool)this.Config["ParallelExtraction"])
                {
                    Parallel.ForEach(fileEntries, entry =>
                    {
                        if(Regex.IsMatch(entry.Key, @"^DATA\/.*_Skins_Skin\d\.bin"))
                        {
                            WritePackedFile(selectedPath, entry.Key, entry.Value);
                        }
                        else
                        {
                            File.WriteAllBytes(Path.Combine(selectedPath, entry.Key), entry.Value);
                        }
                        
                        progress += 0.5;
                        wadExtractor.ReportProgress((int)progress);
                    });
                }
                else
                {
                    foreach (KeyValuePair<string, byte[]> entry in fileEntries)
                    {
                        if (Regex.IsMatch(entry.Key, @"^DATA\/.*_Skins_Skin\d\.bin"))
                        {
                            WritePackedFile(selectedPath, entry.Key, entry.Value);
                        }
                        else
                        {
                            File.WriteAllBytes(Path.Combine(selectedPath, entry.Key), entry.Value);
                        }

                        progress += 0.5;
                        wadExtractor.ReportProgress((int)progress);
                    }
                }
            };

            wadExtractor.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show(string.Format("An error occured:\n{0}", args.Error), "", MessageBoxButton.OK, MessageBoxImage.Error);
                    Logger.Error(string.Format("WAD extraction failed:\n{0}", args.Error));
                }
                else
                {
                    MessageBox.Show("Extraction Successful!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Info("WAD Extraction Successful!");
                }

                this.progressBarWadExtraction.Maximum = 100;
                this.progressBarWadExtraction.Value = 100;
                this.IsEnabled = true;
            };

            wadExtractor.RunWorkerAsync();
        }

        private void WritePackedFile(string selectedPath, string filePath, byte[] data)
        {
            string extension = Path.GetExtension(filePath);
            string basePath = filePath.Substring(0, filePath.IndexOf('_')).Replace("/", "\\");
            string[] packed = Path.GetFileNameWithoutExtension(filePath.Substring(filePath.IndexOf('_') + 1)).Split('_');

            //Checks if packing data file exists, if not create it
            if(!File.Exists(Path.Combine(selectedPath, "OBSIDIAN_PACKING_DATA")))
            {
                File.Create(Path.Combine(selectedPath, "OBSIDIAN_PACKING_DATA"));
            }

            for(int i = 0; i < packed.Length; i += 2)
            {
                string currentPath = string.Format("{0}\\{1}\\{2}{3}", basePath, packed[i], packed[i + 1], extension);
                string fullPath = Path.Combine(selectedPath, currentPath);

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                if(File.Exists(fullPath))
                {
                    CombineBINFiles(new BINFile(fullPath), new BINFile(new MemoryStream(data))).Write(fullPath);
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(selectedPath, currentPath), data);
                }
            }
        }

        private ulong HexStringToUInt64(string hexString)
        {
            byte[] byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return BitConverter.ToUInt64(byteArray, 0);
        }

        private void OpenWADFile(string filePath)
        {
            try
            {
                this.Wad?.Dispose();
                this.Wad = new WADFile(filePath);
            }
            catch (Exception excp)
            {
                Logging.LogException(Logger, "Failed to load WAD File: " + filePath, excp);
                return;
            }

            StringDictionary = new Dictionary<ulong, string>();

            if ((bool)this.Config["GenerateWadDictionary"])
            {
                try
                {
                    WADHashGenerator.GenerateWADStrings(Logger, this.Wad, StringDictionary);
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Failed to Generate WAD String Dictionary", excp);
                }
            }

            this.menuSave.IsEnabled = true;
            this.menuImportHashtable.IsEnabled = true;
            this.menuExportHashtable.IsEnabled = true;
            this.menuExportAll.IsEnabled = true;
            this.menuAddFile.IsEnabled = true;
            this.menuAddFileRedirection.IsEnabled = true;
            this.menuAddFolder.IsEnabled = true;
            this.CurrentlySelectedEntry = null;
            this.datagridWadEntries.ItemsSource = this.Wad.Entries;

            Logger.Info("Opened WAD File: " + filePath);
        }

        private void AddFile(string path, byte[] data, bool compressed, bool refresh)
        {
            using (XXHash64 xxHash = XXHash64.Create())
            {
                ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower())), 0);
                if (!StringDictionary.ContainsKey(hash))
                {
                    StringDictionary.Add(hash, path);
                }

                AddFile(hash, data, compressed, refresh);
            }
        }

        private void AddFile(ulong hash, byte[] data, bool compressed, bool refresh)
        {
            this.Wad.AddEntry(hash, data, compressed);

            if(refresh)
            {
                CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
            }
        }

        private BINFile CombineBINFiles(BINFile binOld, BINFile binNew)
        {
            BINFile bin = binOld;

            foreach(BINFileEntry newEntry in binNew.Entries)
            {
                if (bin.Entries.Exists(entry => entry.Type == newEntry.Type))
                {
                    if(!bin.Entries.Exists(entry => entry.Property == newEntry.Property))
                    {
                        bin.Entries.Add(newEntry);
                    }
                }
                else
                {
                    bin.Entries.Add(newEntry);
                }
            }

            return bin;
        }
    }
}
