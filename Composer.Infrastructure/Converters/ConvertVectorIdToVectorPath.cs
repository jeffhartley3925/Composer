using System;
using System.Linq;
using System.Windows.Data;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Support;
using Composer.Infrastructure.Dimensions;
using System.Windows;

namespace Composer.Infrastructure.Converters
{
    public class ConvertVectorIdToVectorPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int _id;
            string id = "";
            bool boolOut;
            int Id;
            string parameterValue = string.Empty;
            string parameterVariable = string.Empty;
            bool bParameterizedVector = false;
            string path = string.Empty;
            string target = string.Empty;
            try
            {
                if (value != null)
                {
                    id = value.ToString();
                    if (parameter != null)
                    {
                        target = (string)parameter;

                        if (IsMeasureElement(target))
                        {
                            bParameterizedVector = true;
                            parameterVariable = "W";
                            parameterValue = Preferences.MeasureWidth.ToString();
                        }
                        if (target == "StaffDimensionArea")
                        {
                            bParameterizedVector = true;
                            parameterVariable = "W";
                            parameterValue = Preferences.StaffDimensionAreaWidth.ToString();
                        }
                        if (target == StyleTarget.StaffLines_Staff)
                        {
                            bParameterizedVector = true;
                            parameterVariable = "W";
                            parameterValue = Preferences.StaffDimensionAreaWidth.ToString();
                            id = (Preferences.MeasureDebugInfoVisibility == Visibility.Visible) ? "25" : id;
                        }
                    }
                    
                    if (int.TryParse(id, out _id))
                    {
                        path = (from v in Vectors.VectorList
                            where v.Id == _id
                            select v.Path).First();

                        if (bParameterizedVector && path.IndexOf("%" + parameterVariable) > 0)
                        {
                            path = path.Replace("%" + parameterVariable, parameterValue);
                        }
                    }
                    else
                    {
                        switch (target)
                        {
                            case "Dot":
                                if (bool.TryParse(id, out boolOut)) //'id' is a string - 'true' or 'false', not some kind of id.
                                {
                                    if (boolOut)
                                    {
                                        path = (from a in Vectors.VectorList where a.Name == "Dot" select a.Path).First().ToString();
                                    }
                                }
                                break;
                            case "Accidental":
                                if (int.TryParse(id, out Id)) //here, 'id' is an id.
                                {
                                    path = (from a in Accidentals.AccidentalList where a.Id == Id select a.Vector).First().ToString();
                                }
                                break;
                        }

                    }
                }
            }
            catch (Exception)
            {
            }
            return path;
        }

        private bool IsMeasureElement(string target)
        {
            return target.IndexOf(ObjectName.Measure) >= 0 || target == StyleTarget.StaffLines_Measure;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}