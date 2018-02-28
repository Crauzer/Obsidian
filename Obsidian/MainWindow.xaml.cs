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
                try
                {
                    this.Wad?.Dispose();
                    this.Wad = new WADFile(dialog.FileName);
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Failed to load WAD File: " + dialog.FileName, excp);
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

                Logger.Info("Opened WAD File: " + dialog.FileName);
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
                Filter = "Hashtable Files (*.hashtable)|*.hashtable"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    foreach (string fileName in dialog.FileNames)
                    {
                        foreach (string line in File.ReadAllLines(fileName))
                        {
                            string[] lineSplit = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (ulong.TryParse(lineSplit[0], out ulong hash) && !StringDictionary.ContainsKey(hash))
                            {
                                StringDictionary.Add(ulong.Parse(lineSplit[0]), lineSplit[1]);
                            }
                            else
                            {
                                using (XXHash64 xxHash = XXHash64.Create())
                                {
                                    ulong key = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(lineSplit[0].ToLower())), 0);
                                    if (!StringDictionary.ContainsKey(key))
                                    {
                                        StringDictionary.Add(key, lineSplit[0].ToLower());
                                    }
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
                Title = "Select the path to save your Hashtable File",
                Filter = "Hashtable File (*.hashtable)|*.hashtable",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    List<string> lines = new List<string>();
                    foreach (KeyValuePair<ulong, string> pair in StringDictionary)
                    {
                        lines.Add(pair.Key.ToString() + " " + pair.Value);
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
            if (this.buttonAddFileOpen.Visibility == Visibility.Visible)
            {
                try
                {
                    this.Wad.AddEntry(this.textboxAddFilePath.Text, File.ReadAllBytes(this.textboxAddFileFilePath.Text), this.checkboxAddFileCompressed.IsChecked.Value);

                    using (XXHash64 xxHash = XXHash64.Create())
                    {
                        StringDictionary.Add(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(this.textboxAddFilePath.Text.ToLower())), 0), this.textboxAddFilePath.Text);
                    }

                    CollectionViewSource.GetDefaultView(this.datagridWadEntries.ItemsSource).Refresh();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Please choose a different Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                try
                {
                    this.Wad.AddEntry(this.textboxAddFileFilePath.Text, this.textboxAddFilePath.Text);

                    using (XXHash64 xxHash = XXHash64.Create())
                    {
                        StringDictionary.Add(BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(this.textboxAddFileFilePath.Text.ToLower())), 0), this.textboxAddFileFilePath.Text);
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
            this.Wad = new WADFile();

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
                this.Wad = new WADFile();
                StringDictionary = new Dictionary<ulong, string>();

                DirectoryInfo directory = new DirectoryInfo(dialog.SelectedPath);

                using (XXHash64 xxHash = XXHash64.Create())
                {
                    foreach (FileInfo fileInfo in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        string path = fileInfo.FullName.Substring(directory.FullName.Length + 1);
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                        bool hasUnknownPath = fileNameWithoutExtension.All(c => "ABCDEF0123456789".Contains(c)) && fileNameWithoutExtension.Length <= 16;

                        if (hasUnknownPath)
                        {
                            ulong hashedPath = HexStringToUInt64(fileNameWithoutExtension);
                            this.Wad.AddEntry(hashedPath, File.ReadAllBytes(fileInfo.FullName), true);
                        }
                        else
                        {
                            ulong hashedPath = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(path.ToLower())), 0);
                            if (!StringDictionary.ContainsKey(hashedPath))
                            {
                                StringDictionary.Add(hashedPath, path);
                            }

                            this.Wad.AddEntry(hashedPath, File.ReadAllBytes(fileInfo.FullName), true);
                        }
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
                        Directory.CreateDirectory(string.Format("{0}//{1}", selectedPath, Path.GetDirectoryName(entryName)));
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
                    Parallel.ForEach(fileEntries, (entry) =>
                    {
                        File.WriteAllBytes(string.Format("{0}//{1}", selectedPath, entry.Key), entry.Value);
                        progress += 0.5;
                        wadExtractor.ReportProgress((int)progress);
                    });
                }
                else
                {
                    foreach (KeyValuePair<string, byte[]> entry in fileEntries)
                    {
                        File.WriteAllBytes(string.Format("{0}//{1}", selectedPath, entry.Key), entry.Value);
                        progress += 0.5;
                        wadExtractor.ReportProgress((int)progress);
                    }
                }
            };

            wadExtractor.RunWorkerCompleted += (sender, args) =>
            {
                this.IsEnabled = true;
                this.progressBarWadExtraction.Maximum = 100;
                this.progressBarWadExtraction.Value = 100;
                MessageBox.Show("Extraction Succesfull!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.Info("WAD Extraction Successfull!");
            };

            wadExtractor.RunWorkerAsync();
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
    }
}