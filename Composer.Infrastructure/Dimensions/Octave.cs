using System.Linq;
using System.Collections.Generic;

namespace Composer.Infrastructure.Dimensions
{

    public class Octave : DimensionBase
    {
        public short Current { get; set; }

        public static Octave operator +(Octave octave, int increment)
        {
            int current = (octave.Current + increment) % 3;
            return Octaves.OctaveList[current];
        }

        public static Octave operator -(Octave octave, int increment)
        {
            int current = (octave.Current - increment) % 3;
            return Octaves.OctaveList[current];
        }
    }

    public static class Octaves
    {
        public static List<Octave> OctaveList = new List<Octave>();

        public static Octave Octave = null;

        static Octaves()
        {
            Initialize();
        }

        public static void Initialize()
        {
            OctaveList.Add(item: new Octave() { Current = 0, Name = "1" });
            OctaveList.Add(item: new Octave() { Current = 1, Name = "2" });
            OctaveList.Add(item: new Octave() { Current = 2, Name = "3" });
            Octave = OctaveList[0];
        }

        private static Octave ChangeOctave(string octave)
        {
            var o = (from a in OctaveList where a.Name.Trim() == octave select a).DefaultIfEmpty(null).Single();
            if (o != null)
            {
                Octave = o;
            }
            return Octave;
        }

        private static Octave ChangeOctave(int increment)
        {
            return Octave + increment;
        }
    }
}
