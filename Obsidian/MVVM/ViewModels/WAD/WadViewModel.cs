using Fantome.Libraries.League.IO.WadFile;
using HelixToolkit.Wpf;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadViewModel : PropertyNotifier
    {
        public bool ContainsSelection
        {
            get
            {
                foreach (WadItemViewModel item in this.Items)
                {
                    if (item.ContainsSelection)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        public string Filter
        {
            get => this._filter;
            set
            {
                this._filter = value;

                ICollectionView view = CollectionViewSource.GetDefaultView(this.Items);
                view.Filter = itemObject =>
                {
                    WadItemViewModel item = itemObject as WadItemViewModel;

                    //Do this in try-catch because Regex can throw an exception if pattern is wrong
                    try
                    {
                        switch (item)
                        {
                            case WadFileViewModel file:
                            {
                                return Regex.IsMatch(file.Path, value);
                            }
                            case WadFolderViewModel folder:
                            {
                                if (folder.Find(x => Regex.IsMatch(x.Path, value)) == null)
                                {
                                    return false;
                                }
                                else
                                {
                                    folder.Filter = value;
                                    return true;
                                }
                            }
                            default:
                                return false;
                        }
                    }
                    catch(Exception)
                    {
                        return false;
                    }
                };

                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<WadItemViewModel> Items { get; set; } = new ObservableCollection<WadItemViewModel>();

        public Wad WAD
        {
            get
            {
                if (this._wad == null)
                {
                    this._wad = Wad.Mount(this.WADLocation, false);
                }

                return this._wad;
            }
            set => this._wad = value;
        }
        public string WADLocation
        {
            get => this._wadLocation;
            set
            {
                this._wadLocation = value;
                this.WadName = Path.GetFileName(value);
                NotifyPropertyChanged();
            }
        }
        public string WadName 
        {
            get => this._wadName;
            set
            {
                this._wadName = value;
                NotifyPropertyChanged();
            }
        }

        public PreviewViewModel Preview { get; private set; }

        private string _filter;
        private Wad _wad;
        private string _wadLocation;
        private string _wadName;

        public WadViewModel()
        {
            this.Preview = new PreviewViewModel();
        }

        ~WadViewModel()
        {
            CloseWad();
        }

        public void LoadWad(string wadLocation)
        {
            this.WAD = Wad.Mount(wadLocation, false);
            this.WADLocation = wadLocation;

            if(Config.Get<bool>("GenerateHashesFromBIN"))
            {
                Hashtable.Add(HashtableGenerator.Generate(this.WAD));
            }

            GenerateWadItems();
        }
        private void GenerateWadItems()
        {
            foreach (WadEntry entry in this.WAD.Entries.Values)
            {
                string path = Hashtable.Get(entry);
                char pathSeparator = Pathing.GetPathSeparator(path);
                string[] folders = path.Split(pathSeparator);

                //If folders count is 1 then we can assume the file isn't nested in any directory
                if (folders.Length == 1)
                {
                    this.Items.Add(new WadFileViewModel(this, null, path, path, entry));
                }
                else
                {
                    //If folder exists then we pass the file to it
                    //if it doesn't then we create it before passing the file
                    if (this.Items.FirstOrDefault(x => x.Name == folders[0]) is WadFolderViewModel folder)
                    {
                        folder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), path, entry);
                    }
                    else
                    {
                        WadFolderViewModel newFolder = new WadFolderViewModel(this, null, folders[0]);

                        newFolder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), path, entry);
                        this.Items.Add(newFolder);
                    }
                }
            }

            SortItems();
        }
        private void SortItems()
        {
            this.Items.Sort();

            foreach (WadFolderViewModel folder in this.Items.OfType<WadFolderViewModel>())
            {
                folder.Sort();
            }
        }

        public void Refresh()
        {
            //Instead of moving stuff around we can just regenerate the whole tree
            this.Items.Clear();

            GenerateWadItems();
        }

        public void CloseWad()
        {
            this._wad?.Dispose();
            this._wad = null;
        }

        public IEnumerable<WadFileViewModel> GetSelectedFiles()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                switch (item)
                {
                    case WadFileViewModel file when file.IsSelected:
                    {
                        yield return file;
                        break;
                    }
                    case WadFolderViewModel folder:
                    {
                        foreach (WadFileViewModel selectedItem in folder.GetSelectedFiles() ?? Enumerable.Empty<WadFileViewModel>())
                        {
                            yield return selectedItem;
                        }
                        break;
                    }

                }
            }
        }
        public IEnumerable<WadFolderViewModel> GetSelectedFolders()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item is WadFolderViewModel folderItem)
                {
                    foreach(WadFolderViewModel selectedFolder in folderItem.GetSelectedFolders())
                    {
                        yield return selectedFolder;
                    }

                    if(folderItem.IsSelected)
                    {
                        yield return folderItem;
                    }
                }
            }
        }
        public IEnumerable<WadFileViewModel> GetAllFiles()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                switch(item)
                {
                    case WadFileViewModel file:
                    {
                        yield return file;
                        break;
                    }
                    case WadFolderViewModel folder:
                    {
                        foreach (WadFileViewModel childItem in folder.GetAllFiles() ?? Enumerable.Empty<WadFileViewModel>())
                        {
                            yield return childItem;
                        }
                        break;
                    }
                }
            }
        }

        public void NotifySelectionChanged()
        {
            NotifyPropertyChanged(nameof(this.ContainsSelection));
        }
    }
}