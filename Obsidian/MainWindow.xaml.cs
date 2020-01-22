using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SCB;
using Fantome.Libraries.League.IO.SCO;
using Fantome.Libraries.League.IO.SimpleSkin;
using Fantome.Libraries.League.IO.WAD;
using Microsoft.WindowsAPICodePack.Dialogs;
using Obsidian.MVVM;
using Obsidian.MVVM.ViewModels;
using Obsidian.MVVM.ViewModels.WAD;
using Obsidian.Utilities;
using Octokit;
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
            Config.Load();

            InitializeComponent();
            BindMVVM();
        }

        private void BindMVVM()
        {
            this.DataContext = this;
            this.Preview = new PreviewViewModel(this.PreviewViewport);

            DialogHelper.MessageDialog = this.MessageDialog;
            DialogHelper.OperationDialog = this.OperationDialog;
            DialogHelper.RootDialog = this.RootDialog;
        }

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

        private void OnCreateWAD(object sender, RoutedEventArgs e)
        {
            CreateWAD();
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

            //Recursively Remove all WAD entries nested in the folder
            foreach (WadFileViewModel entry in wadFolder.GetAllEntries())
            {
                this.WAD.WAD.RemoveEntry(entry.Entry.XXHash);
            }

            //Remove the folder from View Model
            //If Parent is null then we know it's in root
            if (wadFolder.Parent == null)
            {
                this.WAD.Items.Remove(wadFolder);
            }
            else
            {
                (wadFolder.Parent as WadFolderViewModel).Items.Remove(wadFolder);
            }
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

            this.WAD.WAD.RemoveEntry(wadFile.Entry.XXHash);

            //Remove the file from View Model
            //If Parent is null then we know it's in root
            if (wadFile.Parent == null)
            {
                this.WAD.Items.Remove(wadFile);
            }
            else
            {
                (wadFile.Parent as WadFolderViewModel).Items.Remove(wadFile);
            }
        }

        private void OnPreviewSelectedEntry(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is WadFileViewModel selectedEntry)
            {
                PreviewSelectedEntry(selectedEntry);
            }
            else if (e.NewValue is WadFolderViewModel)
            {

            }
        }

        private async void OpenWad()
        {
            try
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
                {
                    dialog.Multiselect = false;
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

            }
        }
        private async void SaveWad()
        {
            using (CommonSaveFileDialog dialog = new CommonSaveFileDialog())
            {
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

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetAllEntries());
                }
            }
        }
        private async void ExtractSelected()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    await DialogHelper.ShowExtractOperationDialog(dialog.FileName, this.WAD.GetSelectedEntries());
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void OnOperationDialogLoaded(object sender, RoutedEventArgs e)
        {
            await DialogHelper.ShowSyncingHashtableDialog();
        }
    }
}