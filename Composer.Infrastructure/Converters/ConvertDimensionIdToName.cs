using System;
using System.Windows.Data;
using System.Linq;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Converters
{
    public class ConvertDimensionIdToName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var path = string.Empty;
            string dimension;

            try
            {
                if (value != null)
                {
                    if (parameter != null)
                    {
                        var dimensionId = int.Parse(value.ToString());

                        dimension = (string)parameter;

                        if (dimension.StartsWith(DimensionName.Key))
                            dimension = DimensionName.Key;

                        switch (dimension)
                        {
                            case DimensionName.Bar:
                                path = (from a in Dimensions.Bars.BarList where a.Id == dimensionId select a.Name).First();
                                break;
                            case DimensionName.Clef:
                                path = (from a in Dimensions.Clefs.ClefList where a.Id == dimensionId select a.Name).First();
                                break;
                            case DimensionName.Key:
                                path = (from a in Dimensions.Keys.KeyList where a.Id == dimensionId select a.Name).First();
                                break;
                            case DimensionName.TimeSignature:
                                path = (from a in Dimensions.TimeSignatures.TimeSignatureList where a.Id == dimensionId select a.Name).First();
                                break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "ConvertDimensionIdToName");
            }
            return path;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
