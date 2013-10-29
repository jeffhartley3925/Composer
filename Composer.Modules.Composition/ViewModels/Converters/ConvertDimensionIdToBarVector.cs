using System;
using System.Windows.Data;
using System.Linq;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Converters
{
    public class ConvertDimensionIdToBarVector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var path = string.Empty;
            if (value != null)
            {
                if (parameter != null)
                {
                    try
                    {
                        var measure = (Repository.DataService.Measure)value;
                        var barId = measure.Bar_Id;
                        var formatter = (from a in Composer.Infrastructure.Dimensions.Bars.BarList where a.Id == barId select a.Formatter).First();
                        if (string.IsNullOrEmpty(formatter))
                        {
                            path = (from a in Composer.Infrastructure.Dimensions.Bars.BarList where a.Id == barId select a.Vector).First();
                        }
                        else
                        {
                            double barHeight;
                            var mStaff = Utils.GetStaff(measure.Staff_Id);
                            var staffConfiguration = (_Enum.StaffConfiguration)CompositionManager.Composition.StaffConfiguration;
                            var magnitude = (Defaults.BracketHeightBaseline + (EditorState.VerseCount * Defaults.VerseHeight));
                            switch (staffConfiguration)
                            {
                                case _Enum.StaffConfiguration.Grand:
                                    if (mStaff.Sequence == 0)
                                    {
                                        barHeight = magnitude;
                                        path = string.Format(formatter, barHeight, 0);
                                    }
                                    else
                                    {
                                        barHeight = -magnitude + 32;
                                        path = string.Format(formatter, barHeight, 32);
                                    }
                                    break;
                                case _Enum.StaffConfiguration.Simple:
                                    barHeight = Defaults.staffLinesHeight;
                                    path = string.Format(formatter, barHeight, 0);
                                    break;
                                case _Enum.StaffConfiguration.MultiInstrument:
                                    break;
                            }

                        }
                    }
                    catch(Exception)
                    {
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
