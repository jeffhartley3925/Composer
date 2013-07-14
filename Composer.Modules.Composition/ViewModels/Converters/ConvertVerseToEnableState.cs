using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Converters
{
    public class ConvertVerseToEnableState : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;
            Repository.DataService.Verse verse = (Repository.DataService.Verse)value;
            switch((string)parameter)
            {
                case "Inclusion" :
                    result = verse != null;
                    break;
                case "Delete":
                    result = verse != null && verse.Disposition == 1;
                    break;
                case "Up":
                    if (verse != null && verse.Disposition == 1 && EditorState.VerseCount > 1)
                    {
                        result = verse.Index > 1;
                    }
                    break;
                case "Down":
                    if (verse != null && verse.Disposition == 1 && EditorState.VerseCount > 1)
                    {
                        result = verse.Index < EditorState.VerseCount;
                    }
                    break;
                case "Clone":
                    if (verse != null && verse.Disposition == 1)
                    {
                        result = true;
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
