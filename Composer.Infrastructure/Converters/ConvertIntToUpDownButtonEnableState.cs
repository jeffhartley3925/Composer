using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertIntToUpDownButtonEnableState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = "False";
            var index = Int32.Parse(value.ToString());
            var direction = (string)parameter;

            switch (direction)
            {
                case "Up":
                    if (index > 1)
                    {
                        result = "True";
                    }
                    break;
                case "Down":
                    if (index < EditorState.VerseCount)
                    {
                        result = "True";
                    }
                    break;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}