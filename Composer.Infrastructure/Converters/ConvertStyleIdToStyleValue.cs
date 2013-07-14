using System;
using System.Windows.Media;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConverStyleIdToStyleValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Guid guid;
            string style = "";
            try
            {
                if (value != null)
                {
                    if (parameter != null)
                    {
                        string id = value.ToString();

                        if (Guid.TryParse(id, out guid))
                        {
                            string target = (string)parameter;

                            switch (target)
                            {
                                case "MeasureBackground": style = "#f7f7f7"; break;
                                case "MeasureLedgerArea": style = "#EFEFEF"; break;
                                case "StaffLines_Measure": style = "#999999"; break;
                                case "StaffLines_Staff": style = "#999999"; break;
                                case "Note": style = "#999999"; break;
                                case "Selector": style = "Green"; break;
                                case "Cursor": style = "Blue"; break;
                                case "Ledger": style = "#999999"; break;
                                case "StaffDimensionLines": style = "#999999"; break;
                                case "StaffDimensionArea": style = "#f7f7f7"; break;
                                case "StaffDimensionBackground": style = "#f7f7f7"; break;
                                case "StaffDimensionKey": style = "#999999"; break;
                                case "StaffDimensionClef": style = "#999999"; break;
                                case "StaffDimensionTimeSignature": style = "#999999"; break;
                                case "StaffBar": style = "#999999"; break;
                                case "MeasureBar": style = "#999999"; break;

                                default:
                                    style = "#999999";
                                    break;
                            }
                            style = style.Replace("#", "");
                            style = (style.Length == 6) ? "FF" + style : style;

                            if (style.Length == 8)
                            {
                                byte a = System.Convert.ToByte(style.Substring(0, 2), 16);
                                byte r = System.Convert.ToByte(style.Substring(2, 2), 16);
                                byte g = System.Convert.ToByte(style.Substring(4, 2), 16);
                                byte b = System.Convert.ToByte(style.Substring(6, 2), 16);
                                Color color = Color.FromArgb(a, r, g, b);
                                return new SolidColorBrush(color);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return style;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
