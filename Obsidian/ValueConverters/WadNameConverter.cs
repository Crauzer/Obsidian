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
            string finalName = MainWindow.StringDictionary[(value as WADEntry).XXHash];
            if (finalName == null)
            {
                finalName = BitConverter.ToString(BitConverter.GetBytes((value as WADEntry).XXHash)).Replace("-", "");
            }

            return finalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
