using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Composer.Infrastructure.Support;

namespace Composer.Infrastructure.Converters
{
    public class ConvertPaletteTargetToVectorPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = string.Empty;
            try
            {
                if (value != null)
                {
                    var temp = (string)value;
                    var n = temp.Split(',');
                    if (n.Length == 2)
                    {
                        string target = n[0].Trim();
                        string name = n[1].Trim();
                        var query = (from a in Vectors.VectorList where a.Name == name && a.Class == target select a.Path);
                        var enumerable = query as List<string> ?? query.ToList();
                        if (enumerable.Any())
                        {
                            path = enumerable.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, string.Format("ConvertPaletteTargetToVectorPath - Value: {0}; Parameter: {1}", value, parameter));
            }
            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}