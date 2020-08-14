using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using PathIO = System.IO.Path;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadFolderViewModel : WadItemViewModel
    {
        public ObservableCollection<WadItemViewModel> Items { get; set; } = new ObservableCollection<WadItemViewModel>();

        public WadFolderViewModel(WadViewModel wadViewModel, WadItemViewModel parent, string path) :
            base(wadViewModel.Window, wadViewModel, parent, WadItemType.Folder)
        {
            this.Path = path;
            this.Name = PathIO.GetFileName(path);
        }

        public async void AddFile(string fileLocation)
        {
            try
            {
                string entryName = PathIO.GetFileName(fileLocation).ToLower();
                string entryPath = string.Format("{0}/{1}", this.Path, entryName);
                ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(entryPath.ToLower()));
                WADEntry entry = new WADEntry(this._wadViewModel.WAD, hash, File.ReadAllBytes(fileLocation), true, PathIO.GetExtension(fileLocation));

                this.Items.Add(new WadFileViewModel(this._wadViewModel, this, entryPath, entryName, entry));
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog(string.Format("{0}\n{1}\n{2}", Localization.Get("WadFolderAddFileError"), fileLocation, exception));
            }
        }
        public void AddFile(string path, string entryPath, WADEntry entry)
        {
            char pathSeparator = Pathing.GetPathSeparator(path);
            string[] folders = path.Split(pathSeparator);

            //If folders length is 1 then we can add the file to this directory
            //if not, then we pass it down the hierarchy
            if (folders.Length == 1)
            {
                this.Items.Add(new WadFileViewModel(this._wadViewModel, this, entryPath, folders[0], entry));
            }
            else
            {
                //If the folder exists we pass the file to it
                //if it doesn't then we create it before passing the file
                if (this.Items.FirstOrDefault(x => x.Name == folders[0]) is WadFolderViewModel folder)
                {
                    folder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                }
                else
                {
                    string newFolderPath = string.Format("{0}/{1}", this.Path, folders[0]);
                    WadFolderViewModel newFolder = new WadFolderViewModel(this._wadViewModel, this, newFolderPath);

                    newFolder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                    this.Items.Add(newFolder);
                }
            }
        }
        public async void AddFolder(string folderLocation)
        {
            try
            {
                foreach (string fileLocation in Directory.EnumerateFiles(folderLocation, "*", SearchOption.AllDirectories))
                {
                    char pathSeparator = Pathing.GetPathSeparator(fileLocation);
                    string path = fileLocation.Replace(PathIO.GetDirectoryName(folderLocation) + pathSeparator, "").Replace(pathSeparator, '/');
                    string entryPath = string.Format("{0}/{1}", this.Path, path);
                    ulong hash = XXHash.XXH64(Encoding.ASCII.GetBytes(entryPath.ToLower()));
                    WADEntry entry = new WADEntry(this._wadViewModel.WAD, hash, File.ReadAllBytes(fileLocation), true, PathIO.GetExtension(fileLocation));

                    AddFile(path, entryPath, entry);
                }
            }
            catch (Exception exception)
            {
                await DialogHelper.ShowMessageDialog(string.Format("{0}\n{1}", Localization.Get("WadFolderAddFolderError"), exception));
            }
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
                    foreach (WadFolderViewModel selectedFolder in folderItem.GetSelectedFolders())
                    {
                        yield return selectedFolder;
                    }

                    if (folderItem.IsSelected)
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
                switch (item)
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

        public WadFileViewModel Find(Func<WadFileViewModel, bool> predicate)
        {
            return GetAllFiles().FirstOrDefault(predicate);
        }
        
        public bool AreAllItemsSelected()
        {
            if(this.Items.Count == 0)
            {
                return false;
            }

            foreach(WadItemViewModel child in this.Items)
            {
                if(child is WadFolderViewModel childFolder && !childFolder.AreAllItemsSelected())
                {
                    return false;
                }

                if(!child.IsSelected)
                {
                    return false;
                }
            }

            return true;
        }

        public void Sort()
        {
            this.Items.Sort();

            foreach (WadFolderViewModel folder in this.Items.OfType<WadFolderViewModel>())
            {
                folder.Sort();
            }
        }
    }
}