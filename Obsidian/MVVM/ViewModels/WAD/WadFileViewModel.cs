using Fantome.Libraries.League.IO.WAD;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadFileViewModel : WadItemViewModel
    {
        public WADEntry Entry { get; private set; }

        public WadFileViewModel(WadViewModel wadViewModel, WadItemViewModel parent, string path, string name, WADEntry entry)
            : base(wadViewModel, parent, WadItemType.File)
        {
            this.Path = path;
            this.Name = name;
            this.Entry = entry;
        }
    }
}
