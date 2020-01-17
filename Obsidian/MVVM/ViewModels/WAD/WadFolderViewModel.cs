using Fantome.Libraries.League.IO.WAD;
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

        public WadFolderViewModel(string path) : base(WadItemType.Folder)
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
            if(folders.Length == 1)
            {
                this.Items.Add(new WadFileViewModel(entryPath, folders[0], entry));
            }
            else
            {
                WadFolderViewModel folder = this.Items.FirstOrDefault(x => x.Name == folders[0]) as WadFolderViewModel;

                //If the folder exists we pass the file to it
                //if it doesn't then we create it before passing the file
                if(folder != null)
                {
                    folder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                }
                else
                {
                    string newFolderPath = string.Format("{0}/{1}", this.Path, folders[0]);
                    WadFolderViewModel newFolder = new WadFolderViewModel(newFolderPath);

                    newFolder.AddFile(path.Substring(path.IndexOf(pathSeparator) + 1), entryPath, entry);
                    this.Items.Add(newFolder);
                }
            }
        }
    }
}
