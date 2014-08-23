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

        public Subverse(int iX, string mEiD)
        {
            Foreground = Preferences.LyricsPanelVerseForeground;
            Background = Preferences.LyricsPanelVerseBackground;
            MeasureId = mEiD;
            Index = iX;
            VerseText = string.Empty;
            Words = new ObservableCollection<Word>();
        }

        public int Disposition { get; set; }

        public override string ToString()
        {
            var wDs = Words.Aggregate(string.Empty, (current, word) => current + string.Format("{0}{1}", word, Infrastructure.Constants.Defaults.VerseWordDelimitter));
            if (wDs.Length > 0)
            {
                if (wDs.Substring(wDs.Length - 1, 1) == Infrastructure.Constants.Defaults.VerseWordDelimitter.ToString(CultureInfo.InvariantCulture))
                {
                    wDs = wDs.Substring(0, wDs.Length - 1);
                }
            }
            return wDs;
        }
    }
}