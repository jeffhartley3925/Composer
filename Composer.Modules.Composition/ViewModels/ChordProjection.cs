using System;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{

    /// <summary>
    /// 
    /// </summary>
    public class ChordProjection
    {
        private Double Starttime { get; set; }
        public Double NormalizedStarttime { get; set; }
        public int LocationX { get; set; }
        public Guid Id { get; set; }
        private decimal Duration { get; set; }
        private int Index { get; set; }

        public ChordProjection(Guid id, Guid mId, double chSt, int locationX, double mSt, decimal duration, int sg1, int sg2)
        {
            Id = id;
            Starttime = chSt;
            NormalizedStarttime = Normalize(Starttime, mSt, sg1, sg2);
            LocationX = locationX;
            Duration = duration;
            Index = Utils.GetMeasure(mId).Index;
        }

        /// <summary>
        /// Note: This method assumes the MeasureGroup has a vertical density of 2.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
		private double Normalize(double compareChSt, double activeMSt, int compareChSgIndex, int activeChSgIndex)
		{
			var result = compareChSt;
			var staffDuration = (DurationManager.Bpm * Infrastructure.Support.Densities.MeasureDensity);
			if (compareChSgIndex < activeChSgIndex) result = compareChSt + (activeChSgIndex - compareChSgIndex) * staffDuration;
			else if (compareChSt > activeMSt + DurationManager.Bpm) result = compareChSt - (compareChSgIndex - activeChSgIndex) * staffDuration;
			return result;
		}
    }
}
