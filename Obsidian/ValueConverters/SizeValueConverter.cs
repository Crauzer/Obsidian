using System;
using System.Globalization;
using System.Windows.Data;

namespace Obsidian.ValueConverters
{
    public class SizeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            uint uncompressedSize = (uint)value;
            string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            if (uncompressedSize < 0) { return "-" + Convert(-uncompressedSize, targetType, parameter, culture); }
            if (uncompressedSize == 0) { return "0.0 B"; }

            int mag = (int)Math.Log(uncompressedSize, 1024);

            decimal adjustedSize = System.Convert.ToDecimal(value) / (1L << (mag * 10));

            if (Math.Round(adjustedSize, 2) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0} {1}", adjustedSize.ToString("0.##"), sizeSuffixes[mag]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
