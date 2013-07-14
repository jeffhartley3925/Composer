using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertStringToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string sourceString = value.ToString();
            string resultString = string.Empty;
            string target = ((string)parameter).Trim();

            switch (target)
            {
                case "VerseStaggeredDash":
                    return (sourceString == "--") ? "-" : sourceString;
            }

            return resultString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
