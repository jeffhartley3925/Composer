using System;
using System.Windows.Media;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertHexToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                var hexString = ((string)value).Trim();
                hexString = hexString.Replace("#", "");
                if (hexString.Length == 6)
                {
                    hexString = "FF" + hexString;
                }
                if (hexString.Length == 8)
                {
                    var a = System.Convert.ToByte(hexString.Substring(0, 2), 16);
                    var r = System.Convert.ToByte(hexString.Substring(2, 2), 16);
                    var g = System.Convert.ToByte(hexString.Substring(4, 2), 16);
                    var b = System.Convert.ToByte(hexString.Substring(6, 2), 16);
                    var color = Color.FromArgb(a, r, g, b);
                    return new SolidColorBrush(color);
                }
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
