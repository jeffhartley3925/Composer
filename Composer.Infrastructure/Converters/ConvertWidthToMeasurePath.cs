using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertWidthToMeasurePath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString().Length == 0) //can't happen, but....it did once.
            {
                return string.Empty;
            }
            double width = double.Parse(value.ToString());
            string path = string.Format("M 0,1 H {0} Z M 0,9 H {0} Z M 0,17 H {0} Z M 0,25 H {0} Z M 0,33 H {0} Z", width);
            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
