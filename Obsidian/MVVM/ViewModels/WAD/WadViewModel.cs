using Fantome.Libraries.League.IO.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        public ObservableCollection<WadItemViewModel> Items { get; set; } = new ObservableCollection<WadItemViewModel>();

        public WADFile WAD { get; private set; }

        public WadViewModel()
        {

        }

        public void LoadWad(WADFile wad)
        {
            this.WAD = wad;

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

        public IEnumerable<WadFileViewModel> GetSelectedEntries()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item.Type == WadItemType.File && item.IsSelected)
                {
                    yield return item as WadFileViewModel;
                }
                else if (item.Type == WadItemType.Folder)
                {
                    foreach (WadFileViewModel selectedItem in (item as WadFolderViewModel).GetSelectedEntries() ?? Enumerable.Empty<WadFileViewModel>())
                    {
                        yield return selectedItem;
                    }
                }
            }
        }
        public IEnumerable<WadFileViewModel> GetAllEntries()
        {
            foreach (WadItemViewModel item in this.Items)
            {
                if (item.Type == WadItemType.File)
                {
                    yield return item as WadFileViewModel;
                }
                else if (item.Type == WadItemType.Folder)
                {
                    foreach (WadFileViewModel childItem in (item as WadFolderViewModel).GetAllEntries() ?? Enumerable.Empty<WadFileViewModel>())
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
