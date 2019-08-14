using System;
using System.Globalization;
using System.Windows.Data;
using Fantome.Libraries.League.IO.WAD;

namespace Obsidian.ValueConverters
{
    public class WadNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WADEntry wadEntry = value as WADEntry;
            string finalName;
            if (MainWindow.StringDictionary.ContainsKey(wadEntry.XXHash))
            {
                finalName = MainWindow.StringDictionary[wadEntry.XXHash];
            }
            else
            {
                finalName = wadEntry.XXHash.ToString("X16");
            }

            return finalName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
