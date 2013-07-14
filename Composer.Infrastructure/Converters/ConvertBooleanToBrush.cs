using System;
using System.Windows.Media;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertBooleanToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new SolidColorBrush(Colors.Black);
            if (value != null)
            {
                bool outBool;
                if (bool.TryParse(value.ToString(), out outBool))
                {
                    if (!outBool)
                        result = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

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
