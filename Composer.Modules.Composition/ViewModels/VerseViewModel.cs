using System.Collections.Generic;
using System.Globalization;
using Composer.Infrastructure;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Constants;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class VerseViewModel : BaseViewModel, IVerseViewModel
    {
        string metawordFormatter = "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}";
        char dlmtr = Defaults.VerseWordPropertyDelimitter;
        private readonly string _verseIndex;
        private Guid _measureId;

        private ObservableCollection<string> _serializedMetawords;
        public ObservableCollection<string> SerializedMetawords
        {
            get { return _serializedMetawords; }
            set
            {
                _serializedMetawords = value;
                OnPropertyChanged(() => SerializedMetawords);
            }
        }

        private int _disposition;
        public int Disposition
        {
            get { return _disposition; }
            set
            {
                _disposition = value;
                OnPropertyChanged(() => Disposition);
            }
        }

        public VerseViewModel(string verseIndex, string measureId, ObservableCollection<Word> words, int disposition)
        {
            Disposition = disposition;
            _verseIndex = verseIndex;
            _measureId = Guid.Parse(measureId);
            SerializeMetawords(words);
            SubscribeEvents();
        }

        private void SerializeMetawords(IEnumerable<Word> metawords)
        {
            if (metawords == null) throw new ArgumentNullException("metawords");
            ObservableCollection<string> serializedMetawords = new ObservableCollection<string>();
            foreach (var serializedMetaword in metawords.Select(
                metaword => string.Format(  
                    metawordFormatter,
                    metaword.StartTime, dlmtr,
                    metaword.Index, dlmtr,
                    metaword.Text, dlmtr,
                    metaword.Location_X, dlmtr,
                    metaword.MeasureId, dlmtr,
                    metaword.Alignment)))
            {
                serializedMetawords.Add(serializedMetaword);
            }
            SerializedMetawords = serializedMetawords;
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<ArrangeVerse>().Subscribe(OnArrangeVerse);
        }

        public void OnArrangeVerse(Repository.DataService.Measure measure)
        {
            try
            {
                _measureId = measure.Id;
                for (var i = 0; i < SerializedMetawords.Count(); i++)
                {
                    var serializedMetaword = SerializedMetawords[i];
                    var metawordValues = serializedMetaword.Split(Defaults.VerseWordPropertyDelimitter);
                    var startTime = double.Parse(metawordValues[0]);
                    var verseIndex = int.Parse(metawordValues[1]);
                    var word = metawordValues[2];
                    var measureId = metawordValues[4];
                    var alignment = metawordValues[5];
                    var locationX = GetChordXCoordianateFromStartTime(measure, startTime);
                    if (!IsTarget(int.Parse(_verseIndex), measureId))
                        break;
                    SerializedMetawords[i] = string.Format(metawordFormatter, startTime, dlmtr, verseIndex, dlmtr, word, dlmtr, locationX, dlmtr, measureId, dlmtr, alignment);
                }
                SerializedMetawords = SerializedMetawords;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        private static int GetChordXCoordianateFromStartTime(Repository.DataService.Measure measure, double startTime)
        {
            var sgchs = StaffgroupManager.GetAllChordsInStaffgroupByMeasureSequence(measure);
            try
            {
                var b = (from a in sgchs where a.StartTime == startTime select a.Location_X);
                var e = b as List<int> ?? b.ToList();
                if (e.Any())
                {
                    return e.First();
                }
            }
            catch (Exception)
            {
                Exceptions.HandleException(string.Format("Measure Index: {0}, Chord Count: {1}, StartTime: {2}", measure.Index, measure.Chords.Count, startTime));
            }
            return 0;
        }

        private bool IsTarget(int index, string measureId)
        {
            return index.ToString(CultureInfo.InvariantCulture) == _verseIndex && measureId == _measureId.ToString();
        }
    }
}