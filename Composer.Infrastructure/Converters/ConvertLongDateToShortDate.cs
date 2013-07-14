using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertLongDateToShortDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = string.Empty;

            if (value != null)
            {
                var dt = (DateTime)value;
                result = dt.ToShortTimeString() + " " + dt.ToShortDateString();
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
