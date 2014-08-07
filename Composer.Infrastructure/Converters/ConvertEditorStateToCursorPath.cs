using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertEditorStateToCursorPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            string path = string.Empty;
            try
            {
                if (value != null)
                {
                    path = "";
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "ConvertEditorStateToCursorPath");
            }
            return path;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
