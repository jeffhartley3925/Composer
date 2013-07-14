using System.Collections.Generic;

namespace Composer.Infrastructure.Dimensions
{
    public class Accidental : DimensionBase
    {
    }

    public static class Accidentals
    {
        public static List<Accidental> AccidentalList = new List<Accidental>();

        static Accidentals()
        {
            Initialize();
        }

        public static void Initialize()
        {
            AccidentalList.Clear();
            AccidentalList.Add(new Accidental() { Id = (short)Preferences.SharpVectorId, Name = "s", Caption = "Sharp", Description = "", 
                Vector = "M6,46 L3,46 L3,50 L6,50 z M6,38 L7,38 L7,43 L9,43 L9,46 L7,46 L7,50 L9,49 L9,52 L7,52 L7,57 L6,57 L6,53 L3,53 L3,58 L2,58 L2,53 L0,54 L0,51 L2,51 L2,47 L0,47 L0,44 L2,44 L2,40 L3,40 L3,44 L6,43 z" });
            
            AccidentalList.Add(new Accidental() { Id = (short)Preferences.FlatVectorId, Name = "f", Caption = "Flat", Description = "",
                Vector = "M3.072,45.071999 C2.1760001,45.071999 1.4879999,45.552002 1.008,46.512001 L1.008,51.504002 L2.448,50.063999 C2.704,49.807999 2.9359999,49.584 3.1440001,49.391998 C3.352,49.200001 3.52,49.007999 3.648,48.816002 C4.2880001,48.080002 4.6079998,47.344002 4.6079998,46.608002 C4.6079998,46.352001 4.5599999,46.136002 4.4640002,45.959999 C4.368,45.784 4.256,45.616001 4.1280003,45.456001 C4,45.360001 3.8559999,45.272003 3.6960001,45.192001 C3.536,45.112 3.3280001,45.071999 3.072,45.071999 z M4.7047934E-08,33.360001 L1.008,33.360001 L1.008,45.071999 C2,44.368 3.0879998,44.015999 4.2719998,44.015999 C5.0719995,44.015999 5.7919998,44.240002 6.4320002,44.688 C7.072,45.136002 7.3920002,45.743999 7.3920002,46.512001 C7.3920002,47.056 7.152,47.568001 6.6719999,48.048 C6.4799995,48.335999 6.2079997,48.624001 5.8559999,48.912003 C5.5039997,49.200001 5.0879998,49.52 4.6079998,49.872002 L2.112,51.695999 C1.664,51.984001 1.26875,52.288002 0.9262501,52.608002 C0.58375001,52.928001 0.27499998,53.232002 4.7047934E-08,53.52 z"} );

            AccidentalList.Add(new Accidental() { Id = (short)Preferences.NaturalVectorId, Name = "n", Caption = "Natural", Description = "", Vector = "M0 0" });

            AccidentalList.Add(new Accidental()
            {
                Id = (short)Preferences.NullVectorId,
                Name = "b",
                Caption = "None",
                Description = "",
                Vector = "M0 0"
            });
            
            AccidentalList.Add(new Accidental() { Id = 3, Name = "ss", Caption = "Double Sharp", Description = "", Vector = "M0 0" });
            AccidentalList.Add(new Accidental() { Id = 4, Name = "bb", Caption = "Double Flat", Description = "", Vector = "M0 0" });
        }
    }
}
