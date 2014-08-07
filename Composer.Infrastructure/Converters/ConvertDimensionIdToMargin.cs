using System;
using System.Linq;
using System.Windows.Data;
using Composer.Infrastructure.Constants;

namespace Composer.Infrastructure.Converters
{
    public class ConvertDimensionIdToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var margin = string.Empty;
            try
            {
                if (value != null)
                {
                    if (parameter != null)
                    {
                        int dimensionId = int.Parse(value.ToString());
                        var dimensionType = (string)parameter;
                        if (dimensionType.StartsWith(DimensionName.Key)) dimensionType = DimensionName.Key;
                        if (dimensionType.StartsWith(DimensionName.Bar)) dimensionType = DimensionName.Bar;

                        switch (dimensionType)
                        {
                            case DimensionName.Bar:
                                
                                string targetObjectName = ((string)parameter).Split(',')[1].Trim();
                                if (targetObjectName == ObjectName.Staff)
                                {
                                    double topMargin = (41 + Preferences.MeasureTopDeadSpace);
                                    string location = ((string)parameter).Split(',')[2].Trim();
                                    int leftMargin = 0;
                                    if (location == "Right")
                                    {
                                        //if this is the last staff in composition, return the margin for the staff end bar.
                                        if (dimensionId == (from a in Dimensions.Bars.BarList where a.Name == "End" select a.Id).First())
                                        {
                                            leftMargin = -8;
                                            margin = string.Format("{0},{1},0,0", leftMargin, topMargin);
                                        }
                                        else
                                        {
                                            leftMargin = -2;
                                            margin = string.Format("{0},{1},0,0", leftMargin, topMargin);
                                        }
                                    }
                                    else
                                    {
                                        margin = string.Format("{0},{1},0,0", leftMargin, topMargin);
                                    }
                                }
                                else
                                {
                                    margin = string.Format("{0},{1},0,0", Finetune.Measure.BarLeft, Finetune.Measure.BarTop);
                                }
                                break;
                            case DimensionName.Clef:
                                margin = "0,9,0,0";
                                break;
                            case DimensionName.TimeSignature:
                                margin = "0,30,0,0";
                                if (EditorState.IsNewCompositionPanel)
                                {
                                    margin = "10,30,0,0";
                                }
                                break;
                            case DimensionName.Key:
                                margin = "0,0,0,0";
                                if (EditorState.IsNewCompositionPanel)
                                {
                                    margin = "58,6,0,0";
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "ConvertDimensionIdToMargin");
            }
            return margin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}