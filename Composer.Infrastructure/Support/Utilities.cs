using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Composer.Infrastructure.Constants;
using Composer.Repository;

namespace Composer.Infrastructure.Support
{
    public static class Utilities
    {
        public static class Randomness
        {
            private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

            private static readonly Random Rnd = new Random();

            public static int GetRandomInt(int min, int max)
            {
                return Rnd.Next(min, max);
            }

            public static string CreateRandomLengthWord(int minLength, int maxLength)
            {
                var result = string.Empty;
                var wordLength = GetRandomInt(minLength, maxLength);
                for (var k = 0; k < wordLength; k++)
                {
                    var index = GetRandomInt(1, 26);
                    result += Alphabet.Substring(index, 1);
                }
                return result;
            }
        }

        public static class CoordinateSystem
        {
            public static Point TranslateToCompositionCoords(double x, double y, int mseq, int midx, double mstart, double bpm, string mwidth, Guid sid)
            {
                return TranslateCoordinatesToCompositionCoordinates(sid, mseq, midx, Int32.Parse(x.ToString()), Int32.Parse(y.ToString()));
            }

            public static Point TranslateCoordinatesToCompositionCoordinates(Guid sid, int mseq, int midx, int mx, int my)
            {
                var staff = (from a in Cache.Staffs where a.Id == sid select a).SingleOrDefault();

                var widthOfPreviousMeasures = (from a in staff.Measures where a.Sequence < mseq select int.Parse(a.Width)).Sum();
                var compositionXCoord = widthOfPreviousMeasures + mx + Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin;
                
                var previousStaffCount = (int)Math.Floor((double)midx / Composer.Infrastructure.Support.Densities.MeasureDensity);
                var compositionYCoord = my + previousStaffCount * Defaults.StaffHeight - (EditorState.VerseCount * Defaults.VerseHeight);
                return new Point(compositionXCoord, compositionYCoord);
            }
        }

        public static string GetCompositionImageUriFromCompositionId(string guid)
        {
            var protocol = ((Host.Value == "localhost") ? "http" : "https");
            const int collaboratorIndex = 0;
            var compositionImageUrl = string.Format("{0}://{1}/composer/{2}/{3}_{4}.bmp", protocol, Host.Value, Host.CompositionImageDirectory, guid, collaboratorIndex);
            return compositionImageUrl;
        }

        public static string GetCompositionImageUriFromCompositionId(string guid, string collaboratorIndex)
        {
            var protocol = ((Host.Value == "localhost") ? "http" : "https");
            var compositionImageUrl = string.Format("{0}://{1}/composer/{2}/{3}_{4}.bmp", protocol, Host.Value, Host.CompositionImageDirectory, guid, collaboratorIndex);
            return compositionImageUrl;
        }
    }
}
