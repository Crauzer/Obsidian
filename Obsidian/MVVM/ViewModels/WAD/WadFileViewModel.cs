using Fantome.Libraries.League.IO.WAD;
using UtilitiesFantome = Fantome.Libraries.League.Helpers.Utilities;

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

        public string GetInfo()
        {
            return this.Path + '\n'
                + "Compression Type: " + this.Entry.Type.ToString() + '\n'
                + "Compressed Size: " + this.Entry.CompressedSize + '\n'
                + "Uncompressed Size: " + this.Entry.UncompressedSize + '\n'
                + "SHA256: " + UtilitiesFantome.ByteArrayToHex(this.Entry.SHA);
        }
    }
}
