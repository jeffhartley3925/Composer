using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
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
        private static string[] noteCaptions;
        private static double[] noteDurations;
        private static int[] noteSpacings;
        public static ObservableCollection<Duration> Durations;
        private const int definedDurations = 12;

        static DurationManager()
        {

        }

        public static int GetProportionalSpace()
        {
            return (from a in DurationManager.Durations
                    where (a.Caption == EditorState.DurationCaption)
                    select a.Spacing).Single();
        }

        public static int GetProportionalSpace(double duration)
        {
            return (from a in DurationManager.Durations
                    where (a.Value == duration)
                    select a.Spacing).Single();
        }

        public static void Initialize()
        {
            Durations = new ObservableCollection<Duration>();
            noteCaptions = new string[definedDurations] { 
                "Whole", "Half", "Quarter", "Eighth", "Sixteenth", "Thirtysecond", 
                "DottedWhole", "DottedHalf", "DottedQuarter", "DottedEighth", "DottedSixteenth", "DottedThirtysecond" };
            switch (BeatUnit)
            {
                case 2:
                    noteDurations = new double[definedDurations] { 2, 1, .5, .25, .125, .0675, 3, 1.5, .75, .375, .1875, .10125 };
                    break;
                case 4:
                    noteDurations = new double[definedDurations] { 4, 2, 1, .5, .25, .125, 6, 3, 1.5, .75, .375, .1875 };
                    break;
                case 8:
                    noteDurations = new double[definedDurations] { 8, 4, 2, 1, .5, .25, 12, 6, 3, 1.5, .75, .375 };
                    break;
                default: /*default to 4/4 time */
                    noteDurations = new double[definedDurations] { 4, 2, 1, .5, .25, .125, 6, 3, 1.5, .75, .375, .1875 };
                    break;
            }
            //noteSpacings = new int[definedDurations] { 60, 50, 41, 30, 20, 12, 62, 52, 43, 32, 22, 14 };
            noteSpacings = new int[definedDurations] { 68, 62, 52, 44, 32, 20, 68, 62, 52, 44, 32, 20 };
            for (int i = 0; i < definedDurations; i++)
            {
                Durations.Add(new Duration(noteCaptions[i], noteDurations[i], noteSpacings[i], null));
            }
            PopulateNoteVectors();
        }

        public static int GetVectorId(double duration)
        {
            if (duration < 1)
            {
                if (NoteVectors.ContainsKey(duration))
                {
                    int id = int.Parse(NoteVectors[duration]);
                    return id;
                }
            }
            return -1;
        }

        private static void PopulateNoteVectors()
        {
            NoteVectors = new Dictionary<double, string>();

            var vector = (from a in Vectors.VectorList
                          where (a.Class == "Note" &&
                                a.Name == "Thirtysecond")
                          select a).First();

            NoteVectors.Add(.125, vector.Id.ToString());

            vector = (from a in Vectors.VectorList
                      where (a.Class == "Note" &&
                            a.Name == "Sixteenth")
                      select a).First();

            NoteVectors.Add(.25, vector.Id.ToString());

            vector = (from a in Vectors.VectorList
                      where (a.Class == "Note" &&
                            a.Name == "Eighth")
                      select a).First();

            NoteVectors.Add(.5, vector.Id.ToString());

        }
    }
}
