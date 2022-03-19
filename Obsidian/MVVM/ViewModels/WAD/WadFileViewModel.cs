using System;
using LeagueToolkit.Helpers;
using LeagueToolkit.IO.WadFile;
using Obsidian.Utilities;

namespace Obsidian.MVVM.ViewModels.WAD
{
    public class WadFileViewModel : WadItemViewModel
    {
        public LeagueFileType FileType => LeagueToolkit.Helpers.Utilities.GetExtensionType(System.IO.Path.GetExtension(this.Path));
        public FileConversionOptions ConversionOptions => FileConversionHelper.GetFileConversionOptions(this.FileType);
        public WadEntry Entry { get; private set; }

        public WadFileViewModel(WadViewModel wadViewModel, WadItemViewModel parent, string path, string name, WadEntry entry)
            : base(wadViewModel, parent, WadItemType.File)
        {
            this.Path = path;
            this.Name = name;
            this.Entry = entry;
        }

        public string GetInfo()
        {
            return this.Path + '\n'
                + "Compression Type: " + this.Entry.Type.ToString() + '\n'
                + "Compressed Size: " + this.Entry.CompressedSize + '\n'
                + "Uncompressed Size: " + this.Entry.UncompressedSize + '\n'
                + "Checksum: " + Convert.ToHexString(this.Entry.Checksum);
        }
    }
}
