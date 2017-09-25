using Fantome.Libraries.League.Helpers.Utilities;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Obsidian.ValueConverters
{
    public class WadNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Utilities.ByteArrayToHex(BitConverter.GetBytes((ulong)value), true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
