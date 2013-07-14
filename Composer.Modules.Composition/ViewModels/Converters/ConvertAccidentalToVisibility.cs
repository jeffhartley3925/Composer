using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Converters
{
	public class ConvertAccidentalToVisibility : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
		    var result = Visibility.Collapsed;
			if (value != null)
			{
                var note = (Repository.DataService.Note)value;

                if (note.Accidental_Id != null)
                {
                    var accidentalId = (int)note.Accidental_Id;
                    string accidentalSymbol = (from a in Accidentals.AccidentalList where a.Id == accidentalId select a.Name).First();
                    if (accidentalSymbol != EditorState.TargetAccidental)
                    {
                        result = Visibility.Visible;
                    }
                    else if (! EditorState.AccidentalNotes.Contains(note.Pitch.ToCharArray()[0].ToString(CultureInfo.InvariantCulture)))
                    {
                        result = Visibility.Visible;
                    }
                }
			}
			return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
