using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadItemViewModel : PropertyNotifier
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public WadItemType Type { get; }

        public WadItemViewModel(WadItemType type)
        {
            this.Type = type;
        }
    }

    public enum WadItemType
    {
        Folder,
        File
    }
}
