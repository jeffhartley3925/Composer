using System;
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
                string result = string.Empty;
                int wordLength = GetRandomInt(minLength, maxLength);
                for (var k = 0; k < wordLength; k++)
                {
                    int index = GetRandomInt(1, 26);
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

                int widthOfPreviousMeasures = (from a in staff.Measures where a.Sequence < mseq select int.Parse(a.Width)).Sum();
                int compositionXCoord = widthOfPreviousMeasures + mx + Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin;
                
                int previousStaffCount = (int)Math.Floor((double)midx / Composer.Infrastructure.Support.Densities.MeasureDensity);
                int compositionYCoord = my + previousStaffCount * Defaults.StaffHeight - (EditorState.VerseCount * Defaults.VerseHeight);
                return new Point(compositionXCoord, compositionYCoord);
            }
        }

        public static string GetCompositionImageUriFromCompositionId(string guid)
        {
            string protocol = ((Host.Value == "localhost") ? "http" : "https");
            int collaboratorId = 0;
            string compositionImageUrl = string.Format("{0}://{1}/composer/{2}/{3}_{4}.bmp", protocol, Host.Value, Host.CompositionImageDirectory, guid, collaboratorId);
            return compositionImageUrl;
        }
    }
}
