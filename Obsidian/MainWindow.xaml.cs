using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.WAD;
using log4net;
using log4net.Core;
using Microsoft.Win32;
using Obsidian.Utils;
using Obsidian.Windows;
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

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void buttonOpenWadFile_Click(object sender, RoutedEventArgs e)
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

                this.buttonSaveWadFile.IsEnabled = true;
                this.buttonImportHashtable.IsEnabled = true;
                this.buttonExtractHashtable.IsEnabled = true;
                this.buttonAddFile.IsEnabled = true;
                this.butonAddFileRedirection.IsEnabled = true;
                this.CurrentlySelectedEntry = null;
                this.datagridWadEntries.ItemsSource = this.Wad.Entries;

                Logger.Info("Opened WAD File: " + dialog.FileName);
            }
        }

        private void buttonSaveWadFile_Click(object sender, RoutedEventArgs e)
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

        private void buttonImportHashtable_Click(object sender, RoutedEventArgs e)
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

        private void datagridWadEntries_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (this.datagridWadEntries.SelectedItem is WADEntry)
            {
                this.buttonRemoveEntry.IsEnabled = true;
                if (((WADEntry)this.datagridWadEntries.SelectedItem).Type != EntryType.FileRedirection)
                {
                    this.CurrentlySelectedEntry = (WADEntry)this.datagridWadEntries.SelectedItem;
                    this.buttonModifyData.IsEnabled = true;
                }
                else
                {
                    this.CurrentlySelectedEntry = null;
                    this.buttonModifyData.IsEnabled = false;
                }
            }

            this.buttonExtract.IsEnabled = this.datagridWadEntries.SelectedItems.Cast<WADEntry>().ToList().Exists(x => x.Type != EntryType.FileRedirection);
        }

        private void datagridWadEntries_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (((WADEntry)e.Row.DataContext).Type != EntryType.FileRedirection)
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

        private void buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    ExtractWAD(dialog.SelectedPath, this.datagridWadEntries.SelectedItems.Cast<WADEntry>().Where(x => x.Type != EntryType.FileRedirection).ToList());
                }
                catch (Exception excp)
                {
                    Logging.LogException(Logger, "Extraction of the currently opened WAD File failed", excp);
                }
            }
        }

        private void ExtractWAD(string selectedPath, List<WADEntry> selectedEntries)
        {
            this.progressBarWadExtraction.Visibility = Visibility.Visible;
            this.progressBarWadExtraction.Maximum = selectedEntries.Count;
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
                Dictionary<string, byte[]> entries = new Dictionary<string, byte[]>();
                double progress = 0;

                foreach (WADEntry entry in selectedEntries)
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

                    entries.Add(entryName, entryData);
                    progress += 0.5;
                    wadExtractor.ReportProgress((int)progress);
                }

                if ((bool)this.Config["ParallelExtraction"])
                {
                    Parallel.ForEach(entries, (entry) =>
                    {
                        File.WriteAllBytes(string.Format("{0}//{1}", selectedPath, entry.Key), entry.Value);
                        progress += 0.5;
                        wadExtractor.ReportProgress((int)progress);
                    });
                }
                else
                {
                    foreach (KeyValuePair<string, byte[]> entry in entries)
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
                this.progressBarWadExtraction.Value = 0;
                this.progressBarWadExtraction.Visibility = Visibility.Hidden;
                MessageBox.Show("Extraction Succesfull!", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.Info("WAD Extraction Successfull!");
            };

            wadExtractor.RunWorkerAsync();
        }

        private void buttonExtractHashtable_Click(object sender, RoutedEventArgs e)
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
    }
}
