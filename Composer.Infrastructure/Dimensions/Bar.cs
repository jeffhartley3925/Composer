using System.Collections.Generic;

namespace Composer.Infrastructure.Dimensions
{

    public class Bar : DimensionBase
    {
        public string Margin = string.Empty;
    }

    public static class Bars
    {
        public const int StandardBarId = 5;
        public const int EndBarId = 1;
        public const int DoubleBarId = 0;
        public const int BeginRepeatBarId = 2;
        public const int EndRepeatBarId = 3;
        public const int BeginEndRepeatBarId = 4;

        public static List<Bar> BarList = new List<Bar>();

        public static Bar Bar = null;
        public static string BarStaffLinesPathComplement;
        public static string BarStaffLinesPath;
        public static string StaffBarVectorFormatter = "M 0,0 L 0,{0} L 1,{0} L 1,0 Z";
        
        static Bars()
        {
            Initialize();
        }

        public static void Initialize()
        {
            BarStaffLinesPath = "M -5,0 L 0,0 L 0,1 L -5,1 Z M -5,8 L 0,8 L 0,9 L -5,9 Z M -5,16 L 0,16 L 0,17 L -5,17 Z M -5,24 L 0,24 L 0,25 L -5,25 Z M -5,32 L 0,32 L 0,33 L -5,33 Z";

            BarStaffLinesPathComplement = "M -5,26 L 0,26 L 0,27 L -5,27 Z M -5,27 L 0,27 L 0,28 L -5,28 Z M -5,28 L 0,28 L 0,29 L -5,29 Z M -5,29 L 0,29 L 0,30 L -5,30 Z M -5,30 L 0,30 L 0,31 L -5,31 Z M -5,31 L 0,31 L 0,32 L -5,32 Z";
            BarStaffLinesPathComplement += "M -5,18 L 0,18 L 0,19 L -5,19 Z M -5,19 L 0,19 L 0,20 L -5,20 Z M -5,20 L 0,20 L 0,21 L -5,21 Z M -5,21 L 0,21 L 0,22 L -5,22 Z M -5,22 L 0,22 L 0,23 L -5,23 Z M -5,23 L 0,23 L 0,24 L -5,24 Z";
            BarStaffLinesPathComplement += "M -5,10 L 0,10 L 0,11 L -5,11 Z M -5,11 L 0,11 L 0,12 L -5,12 Z M -5,12 L 0,12 L 0,13 L -5,13 Z M -5,13 L 0,13 L 0,14 L -5,14 Z M -5,14 L 0,14 L 0,15 L -5,15 Z M -5,15 L 0,15 L 0,16 L -5,16 Z";
            BarStaffLinesPathComplement += "M -5,1 L 0,1 L 0,2 L -5,2 Z M -5,2 L 0,2 L 0,3 L -5,3 Z M -5,3 L 0,3 L 0,4 L -5,4 Z M -5,4 L 0,4 L 0,5 L -5,5 Z M -5,5 L 0,5 L 0,6 L -5,6 Z M -5,6 L 0,6 L 0,7 L -5,7 Z M -5,7 L 0,7 L 0,8 L -5,8 Z";

            BarStaffLinesPathComplement = "";

            BarList.Clear();
            BarList.Add(new Bar() { Id = 5, Formatter = "M -5,0 L 0,0 L 0,1 L -5,1 Z M -5,8 L 0,8 L 0,9 L -5,9 Z M -5,16 L 0,16 L 0,17 L -5,17 Z M -5,24 L 0,24 L 0,25 L -5,25 Z M -5,32 L 0,32 L 0,33 L -5,33 Z M 0,{1} L 0,{0} L 1,{0} L 1,{1} Z", Margin = "-2,46.5,0,0", Name = "Standard", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,0 L 0,32 L 1,32 L 1,0 Z" });
            BarList.Add(new Bar() { Id = 0, Formatter = "M 0,{1} L 0,{0} 1,{0} 1,{1} M 4,{1} L 4,{0} 5,{0} 5,{1} Z", Margin = "-6,46.5,0,0", Name = "Double", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,0 L 0,32 1,32 1,0 M 4,0 L 4,32 5,32 5,0 Z" });
            BarList.Add(new Bar() { Id = 1, Formatter = "M 0,{1} L 0,{0} 1,{0} 1,{1} M 4,{1} L 4,{0} 8,{0} 8,{1} Z", Margin = "-9,46.5,0,0", Name = "End", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,0 L 0,32 1,32 1,0 M 4,0 L 4,32 8,32 8,0 Z" });
            BarList.Add(new Bar() { Id = 2, Formatter = "", Margin = "-16,46.5,0,0", Name = "BeginRepeat", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,0 L 0,32 4,32 4,0 M 7,0 L 7,32 8,32 8,0 M 10,23 L14,23 L14,19 L10,19 L10,23 M 10,15 L14,15 L14,11 L10,11 L10,15 Z" });
            BarList.Add(new Bar() { Id = 3, Formatter = "", Margin = "-16,46.5,0,0", Name = "EndRepeat", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,23 L4,23 L4,19 L0,19 L0,23 M 0,15 L4,15 L4,11 L0,11 L0,15 M 6,0 L 6,32 7,32 7,0 M 10,0 L 10,32 14,32 14,0 Z" });
            BarList.Add(new Bar() { Id = 4, Formatter = "", Margin = "-25,46.5,0,0", Name = "BeginEndRepeat", Caption = "", Description = "", Vector = BarStaffLinesPath + "M 0,23 L4,23 L4,19 L0,19 L0,23 M 0,15 L4,15 L4,11 L0,11 L0,15 M 6,0 L 6,32 7,32 7,0 M 10,0 L 10,32 14,32 14,0 M 17,0 L 17,32 18,32 18,0 M 19,23 L24,23 L24,19 L20,19 L20,23 M 20,15 L24,15 L24,11 L20,11 L20,15 Z" });
            Bar = BarList[0];
        }
    }
}