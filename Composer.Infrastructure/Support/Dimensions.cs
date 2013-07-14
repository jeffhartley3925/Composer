using System.Collections.Generic;
using Microsoft.Practices.Unity;
using System;
using System.Windows.Media;

namespace Composer.Infrastructure.Support
{
    public static class Dimensions
    {
        public static int StaffgroupDensity;
        public static int MeasureDensity;
        public static int StaffDensity;

        private static int measureCount = 0;
        public static int MeasureCount
        {
            get
            {
                if (measureCount == 0)
                {
                    measureCount = StaffgroupDensity * MeasureDensity * StaffDensity;
                }
                return measureCount;
            }
            set
            {
                measureCount = value;
            }

        }
        public static void Clear()
        {
        }
    }
}