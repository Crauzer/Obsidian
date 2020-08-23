﻿using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.StaticObject;
using Fantome.Libraries.League.IO.SimpleSkin;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM.Commands;
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
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PathIO = System.IO.Path;
using Localization = Obsidian.Utilities.Localization;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using HelixToolkit.Wpf;
using System.Text;

namespace Obsidian
{
#warning When building use the ReleasePortable configuration
#warning Publish with: "dotnet publish -c ReleasePortable -r win-x64 --self-contained true /p:PublishSingleFile=true"

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool IsWadOpened => this.WadViewModels.Count != 0;

        public ObservableCollection<WadViewModel> WadViewModels { get; set; } = new ObservableCollection<WadViewModel>();
        public WadViewModel SelectedWad
        {
            get => this._selectedWad;
            set
            {
                this._selectedWad = value;
                NotifyPropertyChanged(nameof(this.IsWadOpened));
                NotifyPropertyChanged();
            }
        }

        public Dictionary<string, string> LocalizationMap
        {
            get => this._localizationMap;
            set
            {
                this._localizationMap = value;
                NotifyPropertyChanged();
            }
        }

        private WadViewModel _selectedWad;
        private Dictionary<string, string> _localizationMap;
        private int _easterEggClickCounter;

        public ICommand OpenSettingsCommand => new RelayCommand(OpenSettings);

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Config.Load();

            InitializeComponent();
            BindMVVM();
            LoadLocalization();
            CheckForUpdate();
        }

        //Global Exception Handler
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = Localization.Get("FatalErrorMessage") + '\n';
            message += ((Exception)e.ExceptionObject).Message + '\n';
            message += ((Exception)e.ExceptionObject).Source + '\n';
            message += ((Exception)e.ExceptionObject).StackTrace;

            MessageBox.Show(message, Localization.Get("FatalError"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //Initialization functions
        private void BindMVVM()
        {
            this.DataContext = this;

            this.SelectedWad = new WadViewModel(this);

            DialogHelper.Initialize(this);
            DialogHelper.MessageDialog = this.MessageDialog;
            DialogHelper.OperationDialog = this.OperationDialog;
            DialogHelper.RootDialog = this.RootDialog;
        }
        private void LoadLocalization()
        {
            this.LocalizationMap = Localization.Load();
        }
        private async void CheckForUpdate()
        {
            //Do it in try so we don't crash if there isn't internet connection
            try
            {
                GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("Obsidian"));
                IReadOnlyList<Release> releases = await gitClient.Repository.Release.GetAll("Crauzer", "Obsidian");
                Release newestRelease = releases[0];

                // Tags can contain other characters but Version accepts only {x.x.x.x} format
                // this way we can avoid showing the update message for beta versions
                if (Version.TryParse(newestRelease.TagName, out Version newestVersion))
                {
                    // Show update message only if the release version is higher than the one currently executing
                    if (newestVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        await DialogHelper.ShowMessageDialog(Localization.Get("UpdateMessage"));
                        Process.Start("cmd", "/C start https://github.com/Crauzer/Obsidian/releases/tag/" + newestVersion.ToString());
                    }
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

            await DialogHelper.ShowSyncingLocalizationsDialog();
        }

        //Window Utility functions
        private async void OnObsidianImageClickEasterEgg(object sender, MouseButtonEventArgs e)
        {
            this._easterEggClickCounter++;

            if (this._easterEggClickCounter == 10)
            {
                await DialogHelper.ShowMessageDialog("Hmm, what are you doing ?");
            }
            else if (this._easterEggClickCounter == 20)
            {
                await DialogHelper.ShowMessageDialog("You should stop, there's no turning back");
            }
            else if (this._easterEggClickCounter == 30)
            {
                using Stream audioStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Obsidian.Resources.idk.wav");
                using SoundPlayer player = new SoundPlayer(audioStream);

                player.Play();
                await DialogHelper.ShowMessageDialog("You've just unleashed the Wooxy virus");

                this._easterEggClickCounter = 0;
            }
        }
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
                foreach (string file in files)
                {
                    WadViewModel wad = await DialogHelper.ShowOpenWadOperartionDialog(file);

                    this.WadViewModels.Add(wad);

                    this.SelectedWad = wad;
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
                this.SelectedWad.Preview.Clear();
            }

            //Handle multi-selection
            if ((sender as TreeView).IsKeyboardFocusWithin)
            {
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
                            int oldItemIndex = this.SelectedWad.Items.IndexOf(oldItem);
                            int newItemIndex = this.SelectedWad.Items.IndexOf(newItem);
                            int startingIndex = oldItemIndex < newItemIndex ? oldItemIndex : newItemIndex;
                            int endingIndex = oldItemIndex < newItemIndex ? newItemIndex : oldItemIndex;

                            for (int i = startingIndex; i <= endingIndex; i++)
                            {
                                this.SelectedWad.Items[i].IsSelected ^= true;
                            }
                        }
                        else
                        {
                            //Select/Deselect all items in the parent folder in the range
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
                        this.SelectedWad.Preview.Preview(new ImageEngineImage(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                    }
                    catch (FileFormatException)
                    {
                        this.SelectedWad.Preview.Clear();
                        await DialogHelper.ShowMessageDialog(Localization.Get("PreviewErrorDDS"));
                    }
                }
                else if (extension == ".skn")
                {
                    this.SelectedWad.Preview.Preview(new SimpleSkin(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".scb")
                {
                    this.SelectedWad.Preview.Preview(StaticObject.ReadSCB(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".sco")
                {
                    this.SelectedWad.Preview.Preview(StaticObject.ReadSCO(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else if (extension == ".mapgeo")
                {
                    this.SelectedWad.Preview.Preview(new MapGeometry(new MemoryStream(selectedEntry.Entry.GetContent(true))));
                }
                else
                {
                    // Use switch cuz it looks cooler than the shitty formatting for a big if statement
                    switch (extension)
                    {
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                        {
                            using MemoryStream stream = new MemoryStream(selectedEntry.Entry.GetContent(true));
                            BitmapImage bitmap = new BitmapImage();

                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();

                            this.SelectedWad.Preview.Preview(bitmap);
                            break;
                        }
                        case ".json":
                        case ".js":
                        case ".xml":
                        case ".ini":
                        case ".cfg":
                        {
                            using MemoryStream entryStream = new MemoryStream(selectedEntry.Entry.GetContent(true));
                            this.SelectedWad.Preview.PreviewText(entryStream, extension);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.SelectedWad.Preview.Clear();
                await DialogHelper.ShowMessageDialog(Localization.Get("PreviewErrorGeneric"));
            }
        }
        private void RemoveSelectedItems()
        {
            foreach (WadFileViewModel file in this.SelectedWad.GetSelectedFiles().ToList())
            {
                file.Remove();
            }

            foreach (WadFolderViewModel folder in this.SelectedWad.GetSelectedFolders().ToList())
            {
                folder.Remove();
            }
        }

        // ---------------- WAD TAB EVENTS -------------------- \\
        private void OnCloseWadTab(object sender, RoutedEventArgs e)
        {
            WadViewModel wad = (e.Source as Button).DataContext as WadViewModel;

            this.WadViewModels.Remove(wad);
        }
        // DO NOT CHANGE THE FOLLOWING 2 FUNCTIONS PLEASE, THEY ARE A DIRTY WAY
        // OF ENSURING THAT THE VIEWPORT GETS ASSIGNED TO THE WAD PREVIEW
        private void OnWadTabViewportLoaded(object sender, RoutedEventArgs e)
        {
            //wad == null if we're closing the tab
            if (sender is HelixViewport3D viewport && viewport.DataContext is WadViewModel wad)
            {
                wad.Preview.SetViewport(viewport);
            }
        }
        private void OnWadTabViewportDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HelixViewport3D viewport && viewport.DataContext is WadViewModel wad)
            {
                wad.Preview.SetViewport(viewport);
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
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Multiselect = true
            };

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
        private void OnFolderAddFolders(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = true
            };

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
        private void OnFolderRemove(object sender, RoutedEventArgs e)
        {
            WadFolderViewModel wadFolder = (sender as FrameworkElement).DataContext as WadFolderViewModel;

            wadFolder.Remove();
        }

        private async void OnFileModifyData(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Multiselect = false
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WadFileViewModel wadFile = (sender as FrameworkElement).DataContext as WadFileViewModel;

                try
                {
                    wadFile.Entry.EditData(File.ReadAllBytes(dialog.FileName));
                }
                catch (Exception exception)
                {
                    string message = string.Format("{0} \n {1}", Localization.Get("WadEntryModifyDataError"), exception);
                    await DialogHelper.ShowMessageDialog(message);
                }
            }
        }
        private void OnFileRemove(object sender, RoutedEventArgs e)
        {
            WadFileViewModel wadFile = (sender as FrameworkElement).DataContext as WadFileViewModel;

            wadFile.Remove();
        }
        private async void OnFileSaveAs(object sender, RoutedEventArgs e)
        {
            // Check if user actually clicked the menu item
            if (e.OriginalSource is MenuItem menuItem && menuItem.Items.Count == 0)
            {
                string conversionName = menuItem.Header as string;
                WadFileViewModel wadFile = (e.Source as MenuItem).DataContext as WadFileViewModel;
                FileConversion conversion = wadFile.ConversionOptions.GetConversion(conversionName);

                using CommonSaveFileDialog dialog = new CommonSaveFileDialog();
                dialog.Filters.Add(new CommonFileDialogFilter("Converted File", "*" + conversion.OutputExtension));
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    try
                    {
                        FileConversionParameter conversionParameter = conversion.ConstructParameter(dialog.FileName, wadFile, this.SelectedWad);
                        conversion.Convert(conversionParameter);
                    }
                    catch (Exception exception) 
                    {
                        string message = $"{Localization.Get("WadFileConversionError")}\n{exception}";
                        await DialogHelper.ShowMessageDialog(message);
                        return;
                    }
                }
            }
        }

        //Menu event function implementations
        private async void OpenWad()
        {
            try
            {
                using CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.Multiselect = true;
                dialog.InitialDirectory = Config.Get<string>("OpenWadInitialDirectory");
                dialog.Filters.Add(new CommonFileDialogFilter("WAD Files", "*.wad;*.client;*.mobile"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        WadViewModel wad = await DialogHelper.ShowOpenWadOperartionDialog(file);

                        AddWadToTabControl(wad);
                    }
                }
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog(Localization.Get("OpeningWadError") + '\n'
                    + exception.Message + '\n'
                    + exception.StackTrace);
            }
        }
        private async void SaveWad()
        {
            using CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.InitialDirectory = Config.Get<string>("SaveWadInitialDirectory");
            dialog.Filters.Add(new CommonFileDialogFilter("WAD Client File", "*.wad.client"));
            dialog.Filters.Add(new CommonFileDialogFilter("WAD Mobile File", "*.wad.mobile"));
            dialog.Filters.Add(new CommonFileDialogFilter("WAD File", "*.wad"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await DialogHelper.ShowSaveWadOperationDialog(dialog.FileName, this.SelectedWad);
            }
        }

        private async void ExtractAll()
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Config.Get<string>("ExtractInitialDirectory")
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.SelectedWad.GetAllFiles());
            }
        }
        private async void ExtractSelected()
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Config.Get<string>("ExtractInitialDirectory")
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.SelectedWad.GetSelectedFiles());
            }
        }

        private async void ImportHashtable()
        {
            try
            {
                using CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.Multiselect = true;
                dialog.Filters.Add(new CommonFileDialogFilter("Hashtable Files", "*.txt"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    foreach (string hashtableFile in dialog.FileNames)
                    {
                        Hashtable.Load(hashtableFile);
                    }

                    foreach (WadViewModel wad in this.WadViewModels)
                    {
                        wad.Refresh();
                    }
                }
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog(Localization.Get("OpeningHashtablesError") + '\n'
                    + exception.Message + '\n'
                    + exception.StackTrace);
            }
        }
        private void GenerateHashtable()
        {
            using CommonOpenFileDialog wadDialog = new CommonOpenFileDialog();
            wadDialog.Multiselect = true;
            wadDialog.InitialDirectory = Config.Get<string>("OpenWadInitialDirectory");
            wadDialog.Filters.Add(new CommonFileDialogFilter("WAD Files", "*.wad;*.client;*.mobile"));

            if (wadDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                using CommonSaveFileDialog hashtableDialog = new CommonSaveFileDialog();
                hashtableDialog.Filters.Add(new CommonFileDialogFilter("Hashtable File", "*.txt"));
                hashtableDialog.AlwaysAppendDefaultExtension = true;
                hashtableDialog.DefaultExtension = ".txt";

                if (hashtableDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    Dictionary<ulong, string> hashtable = new Dictionary<ulong, string>();

                    foreach (string wadLocation in wadDialog.FileNames)
                    {
                        using WADFile wad = new WADFile(wadLocation);
                        HashtableGenerator.Generate(wad).ToList().ForEach(x =>
                        {
                            if (!hashtable.ContainsKey(x.Key))
                            {
                                hashtable.Add(x.Key, x.Value);
                            }
                        });
                    }

                    Hashtable.Write(hashtableDialog.FileName, hashtable);
                }
            }
        }

        private async void OpenSettings(object o)
        {
            await DialogHelper.ShowSettingsDialog();
        }

        private async void CreateWAD()
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WadViewModel wad = await DialogHelper.ShowCreateWADOperationDialog(dialog.FileName);

                AddWadToTabControl(wad);
            }
        }

        private void AddWadToTabControl(WadViewModel wad)
        {
            this.WadViewModels.Add(wad);

            this.SelectedWad = wad;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}