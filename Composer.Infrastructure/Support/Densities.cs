namespace Composer.Infrastructure.Support
{
    public static class Densities
    {
        public static int StaffgroupDensity;
        public static int MeasureDensity;
        public static int StaffDensity;

        private static int _compositionMeasureCount = 0;
        public static int CompositionCountMeasureCount
        {
            get { return StaffgroupDensity * MeasureDensity * StaffDensity; }
            set { _compositionMeasureCount = value; }
        }

        public static void Clear()
        {

        }
    }
}