using System;
using System.Windows;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertIntToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            if (value != null)
            {
                int i;
                if (int.TryParse(value.ToString(), out i))
                {
                    if (i > 0) result = Visibility.Visible;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
