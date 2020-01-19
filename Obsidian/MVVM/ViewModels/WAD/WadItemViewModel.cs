using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadItemViewModel : PropertyNotifier, IComparable<WadItemViewModel>, IEquatable<WadItemViewModel>, IEqualityComparer<WadItemViewModel>
    {
        public WadItemViewModel Parent { get; }
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
            }
        }
        public string Path { get; set; }
        public string Name { get; set; }
        public WadItemType Type { get; }

        private bool _isSelected;
        protected WadViewModel _wadViewModel;

        public WadItemViewModel(WadViewModel wadViewModel, WadItemViewModel parent, WadItemType type)
        {
            this._wadViewModel = wadViewModel;
            this.Parent = parent;
            this.Type = type;
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
                return this.Name.CompareTo(other.Name);
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
