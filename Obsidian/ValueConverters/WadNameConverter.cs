using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.WAD;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Obsidian.ValueConverters
{
    public class WadNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string finalName = "";
            using (XXHash64 xxHash = XXHash64.Create())
            {
                finalName = MainWindow.StringDictionary.Find(x =>
                BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(x.ToLower())), 0) == (value as WADEntry).XXHash);
            }

            if (finalName == null)
            {
                finalName = Utilities.ByteArrayToHex(BitConverter.GetBytes((ulong)(value as WADEntry).XXHash), true);
            }

            return finalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
