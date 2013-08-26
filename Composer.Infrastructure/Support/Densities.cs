using System.Collections.Generic;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Media;

namespace Composer.Infrastructure.Support
{
    public static class Densities
    {
        public static int StaffgroupDensity;
        public static int MeasureDensity;
        public static int StaffDensity;

        private static int measureCount = 0;
        public static int MeasureCount
        {
            get { return StaffgroupDensity * MeasureDensity * StaffDensity; }
            set { measureCount = value; }
        }

        public static void Clear()
        {
        }
    }
}