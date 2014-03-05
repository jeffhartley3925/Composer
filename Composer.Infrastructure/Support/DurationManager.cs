using System;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Composer.Infrastructure.Support;

namespace Composer.Infrastructure
{
    public static class DurationManager
    {
        public static Dictionary<double, string> NoteVectors;
        public static int BeatUnit;
        public static int Bpm;
        private static string[] _noteCaptions;
        private static double[] _noteDurations;
        private static int[] _noteSpacings;
        public static ObservableCollection<Duration> Durations;
        private const int DefinedDurations = 12;

        public static int GetProportionalSpace()
        {
            //used by MeasureViewModel.GetChordXCoordinate()
            return (from a in Durations
                    where (a.Caption == EditorState.DurationCaption)
                    select a.Spacing).Single();
        }

        public static int GetProportionalSpace(double duration)
        {
            //used by MeasureViewModel.GetRatio()
            return (from a in Durations
                    where (Math.Abs(a.Value - duration) < double.Epsilon)
                    select a.Spacing).Single();
        }

        public static void Initialize()
        {
            Durations = new ObservableCollection<Duration>();
            _noteCaptions = new[] { 
                "Whole", "Half", "Quarter", "Eighth", "Sixteenth", "Thirtysecond", 
                "DottedWhole", "DottedHalf", "DottedQuarter", "DottedEighth", "DottedSixteenth", "DottedThirtysecond" };
            switch (BeatUnit)
            {
                case 2:
                    _noteDurations = new[] { 2, 1, .5, .25, .125, .0675, 3, 1.5, .75, .375, .1875, .10125 };
                    break;
                case 4:
                    _noteDurations = new[] { 4, 2, 1, .5, .25, .125, 6, 3, 1.5, .75, .375, .1875 };
                    break;
                case 8:
                    _noteDurations = new[] { 8, 4, 2, 1, .5, .25, 12, 6, 3, 1.5, .75, .375 };
                    break;
                default: /*default to 4/4 time */
                    _noteDurations = new[] { 4, 2, 1, .5, .25, .125, 6, 3, 1.5, .75, .375, .1875 };
                    break;
            }
            //_noteSpacings = new[] { 68, 62, 52, 44, 32, 20, 68, 62, 52, 44, 32, 20 };
            _noteSpacings = new[] { 50, 45, 40, 35, 30, 25, 50, 45, 40, 35, 30, 25 };
            for (int i = 0; i < DefinedDurations; i++)
            {
                Durations.Add(new Duration(_noteCaptions[i], _noteDurations[i], _noteSpacings[i], null));
            }
            PopulateNoteVectors();
        }

        public static int GetVectorId(double duration)
        {
            if (!(duration < 1)) return -1;
            if (!NoteVectors.ContainsKey(duration)) return -1;
            int id = int.Parse(NoteVectors[duration]);
            return id;
        }

        private static void PopulateNoteVectors()
        {
            NoteVectors = new Dictionary<double, string>();

            var vector = (from a in Vectors.VectorList
                          where (a.Class == "Note" &&
                                a.Name == "Thirtysecond")
                          select a).First();

            NoteVectors.Add(.125, vector.Id.ToString(CultureInfo.InvariantCulture));

            vector = (from a in Vectors.VectorList
                      where (a.Class == "Note" &&
                            a.Name == "Sixteenth")
                      select a).First();

            NoteVectors.Add(.25, vector.Id.ToString(CultureInfo.InvariantCulture));

            vector = (from a in Vectors.VectorList
                      where (a.Class == "Note" &&
                            a.Name == "Eighth")
                      select a).First();

            NoteVectors.Add(.5, vector.Id.ToString(CultureInfo.InvariantCulture));

        }
    }
}
