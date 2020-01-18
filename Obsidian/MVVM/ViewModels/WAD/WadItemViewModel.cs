using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadItemViewModel : PropertyNotifier, IComparable<WadItemViewModel>, IEquatable<WadItemViewModel>
    {
        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                this._isChecked = value;

                if (this.Type == WadItemType.Folder)
                {
                    foreach (WadItemViewModel item in (this as WadFolderViewModel).Items)
                    {
                        item.IsChecked = value;
                    }
                }

                NotifyPropertyChanged();
            }
        }
        public string Path { get; set; }
        public string Name { get; set; }
        public WadItemType Type { get; }

        private bool _isChecked;

        public WadItemViewModel(WadItemType type)
        {
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
    }

    public enum WadItemType
    {
        Folder,
        File
    }
}
