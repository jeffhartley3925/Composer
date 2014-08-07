using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;
using Composer.Modules.Composition.Models;

namespace Composer.Modules.Composition.Models
{
    public sealed class Subverse
    {
        public IEnumerable<Word> Words { get; set; }

        public int? Index { get; set; }

        public string MeasureId { get; set; }

        public string VerseText { get; set; }

        public string Foreground { get; set; }

        public string Background { get; set; }

        public Subverse(int index, string measureId)
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
                if (result.Substring(result.Length - 1, 1) == Infrastructure.Constants.Defaults.VerseWordDelimitter.ToString(CultureInfo.InvariantCulture))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }
    }
}