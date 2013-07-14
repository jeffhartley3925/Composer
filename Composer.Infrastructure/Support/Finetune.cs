
namespace Composer.Infrastructure
{
    public static class Finetune
    {
        public static class Note
        {
            public const string StemUp_X = "0";
            public const string StemDown_X = "4";
            public const string StemUp_Y = "-4";
            public const string StemDown_Y = "26";
        }

        public static class Chord
        {
        }

        public static class Span
        {
            public const string StemUp_X = "-1";
            public const string StemUp_Y = "0";
            public const string StemDown_X = "-10";
            public const string StemDown_Y = "59";
        }

        public static class Measure
        {
            public const int ClickNormalizer_X = -21;
            public const int ClickNormalizer_Y = 0;
            public const int RestLocation_Y = 18;
            public const int BarLeft = -16;
            public const int BarTop = 45;
        }

        public static class NewCompositionPanel
        {
            public const string staff1ComboBoxMarginForSimpleStaffConfiguration = "-27,34,0,0";
            public const string staff1ComboBoxMarginForGrandStaffConfiguration = "0,-6,0,0";

            public const string staff2ComboBoxMarginForGrandStaffConfiguration = "33,238,0,0";

            public const string staffgroupMarginSimpleStaffConfiguration = "0,117,0,0";
            public const string staffgroupMarginGrandStaffConfiguration = "0,77,0,0";
        }

        public static class Staffgroup
        {
            public const string staffgroupBracketMargin = "-10,12,0,0";
        }

        public static class Slot
        {
            public const int Correction_Y = -7;
        }
    }
}
