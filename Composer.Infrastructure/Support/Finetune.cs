
namespace Composer.Infrastructure
{
    public static class Finetune
    {
        public static class Note
        {
            public const string StemUpX = "0";
            public const string StemDownX = "4";
            public const string StemUpY = "-4";
            public const string StemDownY = "26";
        }

        public static class Chord
        {
        }

        public static class Span
        {
            public const string StemUpX = "-1";
            public const string StemUpY = "0";
            public const string StemDownX = "-10";
            public const string StemDownY = "59";
        }

        public static class Measure
        {
            public const int ClickNormalizerX = -21;
            public const int ClickNormalizerY = 0;
            public const int RestLocationY = 18;
            public const int BarLeft = -16;
            public const int BarTop = 45;
        }

        public static class NewCompositionPanel
        {
            public const string Staff1ComboBoxMarginForSimpleStaffConfiguration = "-27,34,0,0";
            public const string Staff1ComboBoxMarginForGrandStaffConfiguration = "0,-6,0,0";

            public const string Staff2ComboBoxMarginForGrandStaffConfiguration = "33,238,0,0";

            public const string StaffgroupMarginSimpleStaffConfiguration = "0,117,0,0";
            public const string StaffgroupMarginGrandStaffConfiguration = "0,77,0,0";
        }

        public static class Staffgroup
        {
            public const string StaffgroupBracketMargin = "-10,12,0,0";
        }

        public static class Slot
        {
            public const int CorrectionY = -7;
        }
    }
}
