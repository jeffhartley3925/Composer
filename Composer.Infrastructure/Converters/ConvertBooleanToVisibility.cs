using System;
using System.Windows;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            if (value != null)
            {
                bool outBool;
                if (bool.TryParse(value.ToString(), out outBool))
                {
                    result = (outBool) ? Visibility.Visible : Visibility.Collapsed;
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
