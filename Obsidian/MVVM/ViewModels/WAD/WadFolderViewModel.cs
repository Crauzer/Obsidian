﻿using Fantome.Libraries.League.IO.WAD;
using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PathIO = System.IO.Path;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadFolderViewModel : WadItemViewModel
    {
        public ObservableCollection<WadItemViewModel> Items { get; set; } = new ObservableCollection<WadItemViewModel>();

        public WadFolderViewModel(WadViewModel wadViewModel, string path) : base(wadViewModel, WadItemType.Folder)
        {
            this.Path = path;
            this.Name = PathIO.GetFileName(path);
        }

        public void AddFile(string path, string entryPath, WADEntry entry)
        {
            char pathSeparator = Pathing.GetPathSeparator(path);
            string[] folders = path.Split(pathSeparator);

            //If folders length is 1 then we can add the file to this directory
            //if not, then we pass it down the hierarchy
            if (folders.Length == 1)
            {
                this.Items.Add(new WadFileViewModel(this._wadViewModel, entryPath, folders[0], entry));
            }
            else
            {
                WadFolderViewModel folder = this.Items.FirstOrDefault(x => x.Name == folders[0]) as WadFolderViewModel;

                //If the folder exists we pass the file to it
                //if it doesn't then we create it before passing the file
                if (folder != null)
                {
                    folder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                }
                else
                {
                    string newFolderPath = string.Format("{0}/{1}", this.Path, folders[0]);
                    WadFolderViewModel newFolder = new WadFolderViewModel(this._wadViewModel, newFolderPath);

                    newFolder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                    this.Items.Add(newFolder);
                }
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
                else if(item.Type == WadItemType.Folder)
                {
                    foreach (WadFileViewModel childItem in (item as WadFolderViewModel).GetAllEntries() ?? Enumerable.Empty<WadFileViewModel>())
                    {
                        yield return childItem;
                    }
                }
            }
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
