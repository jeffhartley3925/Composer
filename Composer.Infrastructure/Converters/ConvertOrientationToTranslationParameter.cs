using System;
using System.Windows.Data;

namespace Composer.Infrastructure.Converters
{
    public class ConvertOrientationToTranslationParameter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "0"; //default to no translation
            if (value == null)
                return result;
            string stemDir = value.ToString();

            int? orientation = Int16.Parse(stemDir);
            string transformType = parameter.ToString();
            try
            {
                if (orientation != (int)_Enum.Orientation.Rest)
                {
                    if (transformType.Length > 0)
                    {
                        switch (transformType)
                        {
                            case "Rotate":
                                result = (orientation == (int)_Enum.Orientation.Up) ? "0" : "180";
                                break;
                            case "TranslateX":
                                result = (orientation == (int)_Enum.Orientation.Up) ? Finetune.Note.StemUpX : Finetune.Note.StemDownX;
                                break;
                            case "TranslateY":
                                result = (orientation == (int)_Enum.Orientation.Up) ? Finetune.Note.StemUpY : Finetune.Note.StemDownY;
                                break;
                            case "TranslateSpanX":
                                result = (orientation == (int)_Enum.Orientation.Up) ? Finetune.Span.StemUpX : Finetune.Span.StemDownX;
                                break;
                            case "TranslateSpanY":
                                result = (orientation == (int)_Enum.Orientation.Up) ? Finetune.Span.StemUpY : Finetune.Span.StemDownY;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "ConvertOrientationToTranslationParameter");
            }
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
