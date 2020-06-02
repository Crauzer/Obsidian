using Fantome.Libraries.League.IO.WAD;
using HelixToolkit.Wpf;
using Obsidian.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadViewModel : PropertyNotifier
    {
        public MainWindow Window { get; }

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

                    if (item.Type == WadItemType.File)
                    {
                        return item.Path.Contains(value);
                    }
                    else
                    {
                        if ((item as WadFolderViewModel).Find(x => x.Path.Contains(value)) == null)
                        {
                            return false;
                        }
                        else
                        {
                            item.Filter = value;
                            return true;
                        }
                    }
                };

                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<WadItemViewModel> Items { get; set; } = new ObservableCollection<WadItemViewModel>();

        public WADFile WAD
        {
            get
            {
                if (this._wad == null)
                {
                    this._wad = new WADFile(this.WADLocation);
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
        private WADFile _wad;
        private string _wadLocation;
        private string _wadName;

        public WadViewModel(MainWindow window)
        {
            this.Window = window;
            this.Preview = new PreviewViewModel();
        }

        public void LoadWad(string wadLocation)
        {
            this.WAD = new WADFile(wadLocation);
            this.WADLocation = wadLocation;

            if(Config.Get<bool>("GenerateHashesFromBIN"))
            {
                Hashtable.Add(HashtableGenerator.Generate(this.WAD));
            }

            GenerateWadItems();
        }
        private void GenerateWadItems()
        {
            foreach (WADEntry entry in this.WAD.Entries)
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
                    WadFolderViewModel folder = this.Items.FirstOrDefault(x => x.Name == folders[0]) as WadFolderViewModel;

                    //If folder exists then we pass the file to it
                    //if it doesn't then we create it before passing the file
                    if (folder != null)
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

        public IEnumerable<WadFileViewModel> GetSelectedFiles()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item.Type == WadItemType.File && item.IsSelected)
                {
                    yield return item as WadFileViewModel;
                }
                else if (item.Type == WadItemType.Folder)
                {
                    foreach (WadFileViewModel selectedItem in (item as WadFolderViewModel).GetSelectedFiles() ?? Enumerable.Empty<WadFileViewModel>())
                    {
                        yield return selectedItem;
                    }
                }
            }
        }
        public IEnumerable<WadFolderViewModel> GetSelectedFolders()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item.Type == WadItemType.Folder)
                {
                    foreach(WadFolderViewModel selectedFolder in (item as WadFolderViewModel).GetSelectedFolders())
                    {
                        yield return selectedFolder;
                    }

                    if(item.IsSelected)
                    {
                        yield return item as WadFolderViewModel;
                    }
                }
            }
        }
        public IEnumerable<WadFileViewModel> GetAllFiles()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item.Type == WadItemType.File)
                {
                    yield return item as WadFileViewModel;
                }
                else if (item.Type == WadItemType.Folder)
                {
                    foreach (WadFileViewModel childItem in (item as WadFolderViewModel).GetAllFiles() ?? Enumerable.Empty<WadFileViewModel>())
                    {
                        yield return childItem;
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