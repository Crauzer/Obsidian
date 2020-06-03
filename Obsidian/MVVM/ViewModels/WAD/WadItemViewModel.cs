using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using PathIO = System.IO.Path;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadItemViewModel : PropertyNotifier, IComparable<WadItemViewModel>, IEquatable<WadItemViewModel>, IEqualityComparer<WadItemViewModel>
    {
        public MainWindow Window { get; }
        public WadItemViewModel Parent { get; }
        public string Filter
        {
            get => this._filter;
            set
            {
                this._filter = value;
                if (this.Type == WadItemType.Folder)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView((this as WadFolderViewModel).Items);
                    view.Filter = itemObject =>
                    {
                        WadItemViewModel item = itemObject as WadItemViewModel;

                        if (item.Type == WadItemType.File)
                        {
                            return Regex.IsMatch(item.Path, value);
                        }
                        else
                        {
                            if ((item as WadFolderViewModel).Find(x => Regex.IsMatch(x.Path, value)) == null)
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
                }

                NotifyPropertyChanged();
            }
        }
        public bool ContainsSelection
        {
            get
            {
                if (this.IsSelected)
                {
                    return true;
                }
                else if (this.Type == WadItemType.Folder)
                {
                    foreach (WadItemViewModel item in (this as WadFolderViewModel).Items)
                    {
                        if (item.ContainsSelection)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                this._isSelected = value;

                if (this.Type == WadItemType.Folder)
                {
                    foreach (WadItemViewModel item in (this as WadFolderViewModel).Items)
                    {
                        item.IsSelected = value;
                    }
                }

                this._wadViewModel.NotifySelectionChanged();
                NotifyPropertyChanged();
                NotifySelectionChanged();
            }
        }
        public string Tooltip
        {
            get
            {
                if (this.Type == WadItemType.File)
                {
                    return (this as WadFileViewModel).GetInfo();
                }
                else
                {
                    return this.Path;
                }
            }
        }
        public string Path { get; set; }
        public string Name { get; set; }
        public WadItemType Type { get; }

        private bool _isSelected;
        private string _filter;
        protected WadViewModel _wadViewModel;

        public WadItemViewModel(MainWindow window, WadViewModel wadViewModel, WadItemViewModel parent, WadItemType type)
        {
            this.Window = window;
            this._wadViewModel = wadViewModel;
            this.Parent = parent;
            this.Type = type;
        }

        public void Remove()
        {
            if(this.Type == WadItemType.File)
            {
                this._wadViewModel.WAD.RemoveEntry((this as WadFileViewModel).Entry.XXHash);
            }
            else
            {
                WadFolderViewModel wadFolder = this as WadFolderViewModel;

                //Recursively Remove all WAD entries nested in the folder
                foreach (WadFileViewModel entry in wadFolder.GetAllFiles())
                {
                    this._wadViewModel.WAD.RemoveEntry(entry.Entry.XXHash);
                }
            }

            //Remove the item from View Model
            //If Parent is null then we know it's in root
            if (this.Parent == null)
            {
                this._wadViewModel.Items.Remove(this);
            }
            else
            {
                (this.Parent as WadFolderViewModel).Items.Remove(this);
            }
        }

        public void NotifySelectionChanged()
        {
            if (this.Type == WadItemType.Folder)
            {
                bool are = (this as WadFolderViewModel).AreAllItemsSelected();
                if (are)
                {
                    this._isSelected = true;
                }
                else
                {
                    this._isSelected = false;
                }
            }

            if (this.Parent != null)
            {
                (this.Parent as WadFolderViewModel).NotifySelectionChanged();
            }

            NotifyPropertyChanged(nameof(this.IsSelected));
        }

        public int CompareTo(WadItemViewModel other)
        {
            if (this.Type == WadItemType.Folder && other.Type == WadItemType.File)
            {
                return -1;
            }
            else if (this.Type == WadItemType.File && other.Type == WadItemType.Folder)
            {
                return 1;
            }
            else
            {
                string extensionThis = PathIO.GetExtension(this.Path);
                string extensionOther = PathIO.GetExtension(other.Path);

                if (extensionThis == extensionOther)
                {
                    return this.Name.CompareTo(other.Name);
                }
                else
                {
                    return extensionThis.CompareTo(extensionOther);
                }
            }
        }
        public bool Equals(WadItemViewModel other)
        {
            return this.Path == other.Path;
        }
        public bool Equals(WadItemViewModel x, WadItemViewModel y)
        {
            return x.Path == y.Path;
        }

        public int GetHashCode(WadItemViewModel obj)
        {
            return this.Path.GetHashCode();
        }
    }

    public enum WadItemType
    {
        Folder,
        File
    }
}