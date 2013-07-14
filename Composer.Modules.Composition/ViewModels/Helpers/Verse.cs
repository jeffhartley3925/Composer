using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class Verse
    {
        public ObservableCollection<Word> Words { get; set; }

        public int? Index { get; set; }

        public string MeasureId { get; set; }

        public string VerseText { get; set; }

        public string Foreground { get; set; }

        public string Background { get; set; }

        public Verse(int index, string measureId)
        {
            Foreground = Preferences.LyricsPanelVerseForeground;
            Background = Preferences.LyricsPanelVerseBackground;
            MeasureId = measureId;
            Index = index;
            VerseText = string.Empty;
            Words = new ObservableCollection<Word>();
        }

        public int Disposition { get; set; }

        public override string ToString()
        {
            var result = Words.Aggregate(string.Empty, (current, word) => current + string.Format("{0}{1}", word, Infrastructure.Constants.Defaults.VerseWordDelimitter));
            if (result.Length > 0)
            {
                //TODO: sometimes result.length == 0 which causes an error here.
                if (result.Substring(result.Length - 1, 1) == Infrastructure.Constants.Defaults.VerseWordDelimitter.ToString(CultureInfo.InvariantCulture))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }
    }
}