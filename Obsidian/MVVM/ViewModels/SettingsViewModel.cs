using Obsidian.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Obsidian.MVVM.ViewModels
{
    public class SettingsViewModel
    {
        public string OpenWadInitialDirectory
        {
            get => Config.Get<string>("OpenWadInitialDirectory");
            set
            {
                Config.Set("OpenWadInitialDirectory", value);
            }
        }
        public string SaveWadInitialDirectory
        {
            get => Config.Get<string>("SaveWadInitialDirectory");
            set
            {
                Config.Set("SaveWadInitialDirectory", value);
            }
        }
        public string ExtractInitialDirectory
        {
            get => Config.Get<string>("ExtractInitialDirectory");
            set
            {
                Config.Set("ExtractInitialDirectory", value);
            }
        }
    }
}
