using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SCB;
using Fantome.Libraries.League.IO.SCO;
using Fantome.Libraries.League.IO.SimpleSkin;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM.ViewModels;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PathIO = System.IO.Path;

namespace Obsidian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool IsWadOpened
        {
            get => this._isWadOpened;
            set
            {
                this._isWadOpened = value;
                NotifyPropertyChanged();
            }
        }
        public WadViewModel WAD
        {
            get => this._wad;
            private set
            {
                this._wad = value;
                this.IsWadOpened = true;
                NotifyPropertyChanged();
            }
        }
        public PreviewViewModel Preview { get; private set; }

        private WadViewModel _wad = new WadViewModel();
        private bool _isWadOpened;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Config.Load();

            InitializeComponent();
            BindMVVM();
            CheckForUpdate();
        }

        //Global Exception Handler
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = "A Fatal Error has occurred, Obsidian will now terminate.\n";
            message += ((Exception)e.ExceptionObject).Message + '\n';
            message += ((Exception)e.ExceptionObject).Source + '\n';
            message += ((Exception)e.ExceptionObject).StackTrace;

            MessageBox.Show(message, "Obsidian - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //Initialization functions
        private void BindMVVM()
        {
            this.DataContext = this;
            this.Preview = new PreviewViewModel(this.PreviewViewport);

            DialogHelper.MessageDialog = this.MessageDialog;
            DialogHelper.OperationDialog = this.OperationDialog;
            DialogHelper.RootDialog = this.RootDialog;
        }
        private async void CheckForUpdate()
        {
            //Do it in try so we don't crash if there isn't internet connection
            try
            {
                GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("Obsidian"));

                IReadOnlyList<Release> releases = await gitClient.Repository.Release.GetAll("Crauzer", "Obsidian");
                Release newestRelease = releases[0];
                Version newestVersion = new Version(newestRelease.TagName);

                if (newestVersion > Assembly.GetExecutingAssembly().GetName().Version)
                {
                    await DialogHelper.ShowMessageDialog("A new version of Obsidian is available." + '\n' + @"Click the ""OK"" button to download it.");
                    Process.Start("cmd", "/C start https://github.com/Crauzer/Obsidian/releases/tag/" + newestVersion.ToString());
                }
            }
            catch (Exception) { }
        }
        private async void OnOperationDialogLoaded(object sender, RoutedEventArgs e)
        {
            if (Config.Get<bool>("SyncHashes"))
            {
                await DialogHelper.ShowSyncingHashtableDialog();
            }
        }

        //Window Utility functions
        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                RemoveSelectedItems();
            }
        }
        private async void OnWindowDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length != 1)
                {
                    await DialogHelper.ShowMessageDialog("You cannot drop more than 1 WAD file into Obsidian");
                }
                else
                {
                    this.WAD = await DialogHelper.ShowOpenWadOperartionDialog(files[0]);
                    this.Preview.Clear();
                }
            }
        }
        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //Handle preview
            if (e.NewValue is WadFileViewModel selectedEntry)
            {
                PreviewSelectedEntry(selectedEntry);
            }
            else if (e.NewValue is WadFolderViewModel)
            {
                this.Preview.Clear();
            }

            //Handle multi-selection
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                (e.NewValue as WadItemViewModel).IsSelected ^= true;
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                WadItemViewModel oldItem = e.OldValue as WadItemViewModel;
                WadItemViewModel newItem = e.NewValue as WadItemViewModel;

                //Handle batch selection only if parent of both items is the same
                if (oldItem.Parent == newItem.Parent)
                {
                    //If they are in the root then we access the WadViewModel
                    if (oldItem.Parent == null)
                    {
                        //Select/Deselect all items in the range
                        int oldItemIndex = this.WAD.Items.IndexOf(oldItem);
                        int newItemIndex = this.WAD.Items.IndexOf(newItem);
                        int startingIndex = oldItemIndex < newItemIndex ? oldItemIndex : newItemIndex;
                        int endingIndex = oldItemIndex < newItemIndex ? newItemIndex : oldItemIndex;

                        for (int i = startingIndex; i <= endingIndex; i++)
                        {
                            this.WAD.Items[i].IsSelected ^= true;
                        }
                    }
                    else
                    {
                        //Select/Deselect all items in parent the parent folder in the range
                        WadFolderViewModel parent = oldItem.Parent as WadFolderViewModel;
                        int oldItemIndex = parent.Items.IndexOf(oldItem);
                        int newItemIndex = parent.Items.IndexOf(newItem);
                        int startingIndex = oldItemIndex < newItemIndex ? oldItemIndex : newItemIndex;
                        int endingIndex = oldItemIndex < newItemIndex ? newItemIndex : oldItemIndex;

                        for (int i = startingIndex; i <= endingIndex; i++)
                        {
                            parent.Items[i].IsSelected ^= true;
                        }
                    }
                }
            }
        }
        private async void PreviewSelectedEntry(WadFileViewModel selectedEntry)
        {
            string extension = PathIO.GetExtension(selectedEntry.Path);

            try
            {
                if (extension == ".dds")
                {
                    try
                    {
                        this.Preview.Preview(new ImageEngineImage(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                    }
                    catch (FileFormatException)
                    {
                        await DialogHelper.ShowMessageDialog("Previewing of this DDS Format is not supported\n" +
                            "File is most likely a cubemap or has a dimension of size 1");
                    }
                }
                else if (extension == ".skn")
                {
                    this.Preview.Preview(new SKNFile(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".scb")
                {
                    this.Preview.Preview(new SCBFile(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".sco")
                {
                    this.Preview.Preview(new SCOFile(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".mapgeo")
                {
                    this.Preview.Preview(new MGEOFile(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
            }
            catch (Exception)
            {
                await DialogHelper.ShowMessageDialog("Unable to preview the selected file");
            }
        }
        private void RemoveSelectedItems()
        {
            foreach (WadFileViewModel file in this.WAD.GetSelectedFiles().ToList())
            {
                file.Remove();
            }

            foreach (WadFolderViewModel folder in this.WAD.GetSelectedFolders().ToList())
            {
                folder.Remove();
            }
        }

        //Menu event functions
        private void OnWadOpen(object sender, RoutedEventArgs e)
        {
            OpenWad();
        }
        private void OnWadSave(object sender, RoutedEventArgs e)
        {
            SaveWad();
        }

        private void OnExtractAll(object sender, RoutedEventArgs e)
        {
            ExtractAll();
        }
        private void OnExtractSelected(object sender, RoutedEventArgs e)
        {
            ExtractSelected();
        }

        private void OnImportHashtable(object sender, RoutedEventArgs e)
        {
            ImportHashtable();
        }

        private void OnCreateWAD(object sender, RoutedEventArgs e)
        {
            CreateWAD();
        }

        private void OnGenerateHashtable(object sender, RoutedEventArgs e)
        {
            GenerateHashtable();
        }

        private void OnFolderAddFiles(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    WadFolderViewModel wadFolder = (sender as FrameworkElement).DataContext as WadFolderViewModel;

                    foreach (string fileLocation in dialog.FileNames)
                    {
                        wadFolder.AddFile(fileLocation);
                    }

                    wadFolder.Sort();
                }
            }
        }
        private void OnFolderAddFolders(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    WadFolderViewModel wadFolder = (sender as FrameworkElement).DataContext as WadFolderViewModel;

                    foreach (string folderLocation in dialog.FileNames)
                    {
                        wadFolder.AddFolder(folderLocation);
                    }

                    wadFolder.Sort();
                }
            }
        }
        private void OnFolderRemove(object sender, RoutedEventArgs e)
        {
            WadFolderViewModel wadFolder = (sender as FrameworkElement).DataContext as WadFolderViewModel;

            wadFolder.Remove();
        }

        private void OnFileModifyData(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    WadFileViewModel wadFile = (sender as FrameworkElement).DataContext as WadFileViewModel;

                    try
                    {
                        wadFile.Entry.EditData(File.ReadAllBytes(dialog.FileName));
                    }
                    catch (Exception exception)
                    {

                    }
                }
            }
        }
        private void OnFileRemove(object sender, RoutedEventArgs e)
        {
            WadFileViewModel wadFile = (sender as FrameworkElement).DataContext as WadFileViewModel;

            wadFile.Remove();
        }

        //Menu event function implementations
        private async void OpenWad()
        {
            try
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.InitialDirectory = Config.Get<string>("OpenWadInitialDirectory");
                    dialog.Filters.Add(new CommonFileDialogFilter("WAD Files", "*.wad;*.client;*.mobile"));

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        this.WAD = await DialogHelper.ShowOpenWadOperartionDialog(dialog.FileName);
                        this.Preview.Clear();
                    }
                }
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog("Obsidian was unable to open the WAD file you selected\n"
                    + exception.Message + '\n'
                    + exception.StackTrace);
            }
        }
        private async void SaveWad()
        {
            using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
            {
                dialog.InitialDirectory = Config.Get<string>("SaveWadInitialDirectory");
                dialog.AlwaysAppendDefaultExtension = true;
                dialog.DefaultExtension = ".client";
                dialog.Filters.Add(new CommonFileDialogFilter("wad.client File", "*.client"));
                dialog.Filters.Add(new CommonFileDialogFilter("wad File", "*.wad"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string wadLocation = dialog.FileName;

                    //We need to change the extension because the dialog is stupid 
                    //and can't handle extensions with multiple dots
                    if (PathIO.GetExtension(dialog.FileName) == ".client")
                    {
                        wadLocation = PathIO.ChangeExtension(dialog.FileName, ".wad.client");
                    }

                    await DialogHelper.ShowSaveWadOperationDialog(wadLocation, this.WAD);
                }
            }
        }

        private async void ExtractAll()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.InitialDirectory = Config.Get<string>("ExtractInitialDirectory");

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetAllFiles());
                }
            }
        }
        private async void ExtractSelected()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.InitialDirectory = Config.Get<string>("ExtractInitialDirectory");

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetSelectedFiles());
                }
            }
        }

        private async void ImportHashtable()
        {
            try
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Filters.Add(new CommonFileDialogFilter("Hashtable Files", "*.txt"));

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        foreach (string hashtableFile in dialog.FileNames)
                        {
                            Hashtable.Load(hashtableFile);
                        }

                        this.WAD.Refresh();
                    }
                }
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog("Obsidian was unable to open the hashtables you selected\n"
                    + exception.Message + '\n'
                    + exception.StackTrace);
            }
        }
        private void GenerateHashtable()
        {
            using (CommonOpenFileDialog wadDialog = new CommonOpenFileDialog())
            {
                wadDialog.Multiselect = true;
                wadDialog.InitialDirectory = Config.Get<string>("OpenWadInitialDirectory");
                wadDialog.Filters.Add(new CommonFileDialogFilter("WAD Files", "*.wad;*.client;*.mobile"));

                if (wadDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    using (CommonSaveFileDialog hashtableDialog = new CommonSaveFileDialog())
                    {
                        hashtableDialog.Filters.Add(new CommonFileDialogFilter("Hashtable File", "*.txt"));
                        hashtableDialog.AlwaysAppendDefaultExtension = true;
                        hashtableDialog.DefaultExtension = ".txt";

                        if (hashtableDialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            Dictionary<ulong, string> hashtable = new Dictionary<ulong, string>();

                            foreach (string wadLocation in wadDialog.FileNames)
                            {
                                using (WADFile wad = new WADFile(wadLocation))
                                {
                                    HashtableGenerator.Generate(wad).ToList().ForEach(x =>
                                    {
                                        if (!hashtable.ContainsKey(x.Key))
                                        {
                                            hashtable.Add(x.Key, x.Value);
                                        }
                                    });
                                }
                            }

                            Hashtable.Write(hashtableDialog.FileName, hashtable);
                        }
                    }
                }
            }
        }

        private async void CreateWAD()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    this.WAD = await DialogHelper.ShowCreateWADOperationDialog(dialog.FileName);
                    this.Preview.Clear();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}