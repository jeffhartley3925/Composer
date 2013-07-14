using System;
using System.Windows.Data;
using System.Linq;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Converters
{
    public class ConvertDimensionIdToVectorPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isPlainBar = false;
            var path = string.Empty;

            if (value != null)
            {
                if (parameter != null)
                {
                    var dimensionId = int.Parse(value.ToString());
                    var dimension = (string)parameter;

                    if (dimension.StartsWith(DimensionName.Bar))
                    {
                        if (dimension != "Bar")
                        {
                            isPlainBar = true;
                        }
                        dimension = DimensionName.Bar;
                    }

                    if (dimension.StartsWith(DimensionName.Key))
                        dimension = DimensionName.Key;

                    if (dimension.StartsWith(DimensionName.Clef))
                        dimension = DimensionName.Clef;

                    switch (dimension)
                    {
                        case DimensionName.Bar:
                            path = (from a in Dimensions.Bars.BarList where a.Id == dimensionId select a.Vector).First();
                            int pos = path.IndexOf(Dimensions.Bars.BarStaffLinesPath, StringComparison.Ordinal);
                            if (pos >= 0 && isPlainBar)
                            {
                                //if this is the Bars comboBox then strip off the stub staff lines.
                                path = path.Substring(Dimensions.Bars.BarStaffLinesPath.Length);
                            }
                            break;
                        case DimensionName.Clef:
                            path = (from a in Dimensions.Clefs.ClefList where a.Id == dimensionId select a.Vector).First();
                            break;
                        case DimensionName.Key:
                            path = (from a in Dimensions.Keys.KeyList where a.Id == dimensionId select a.Vector).First();
                            break;
                        case DimensionName.TimeSignature:
                            path = (from a in Dimensions.TimeSignatures.TimeSignatureList where a.Id == dimensionId select a.Vector).First();
                            break;
                    }
                }
            }

            return path;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
