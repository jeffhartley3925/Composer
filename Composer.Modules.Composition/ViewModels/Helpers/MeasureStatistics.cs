using System;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
	using System.Threading;

	public static class Statistics
    {
        public static List<MeasureStatistics> CompositionMeasureStatistics = null;

        public static void AddMeasureStatistics(Repository.DataService.Measure mE)
        {
	        if (mE.Chords.Count == 0) return;
	        if (CompositionMeasureStatistics == null)
	        {
		        CompositionMeasureStatistics = new List<MeasureStatistics>();
	        }
	        else RemoveMeasureStatistics(mE.Id);

	        foreach (var cLn in CompositionManager.Composition.Collaborations)
            {
                CompositionMeasureStatistics.Add(new MeasureStatistics(mE, cLn));
            }
        }

        public static void RemoveMeasureStatistics(Guid mEiD)
        {
			CompositionMeasureStatistics.RemoveAll(x => x.MeasureId == mEiD);
        }

		public static void UpdateCompositionMeasureStatistics(Guid mEiD)
        {
            var mE = Utils.GetMeasure(mEiD);
            RemoveMeasureStatistics(mEiD);
            AddMeasureStatistics(mE);
        }
    }

    public class MeasureStatistics
    {
        public Guid MeasureId { get; set; }
        public int? MeasureIndex { get; set; }
        public int CollaboratorIndex { get; set; }
		public double MeasureDuration { get; set; }
        public bool IsPackedMeasure { get; set; }
        public bool IsInPackedMeasuregroup { get; set; }

        public MeasureStatistics(Repository.DataService.Measure mE, Repository.DataService.Collaboration cLn)
        {
            MeasureId = mE.Id;
            MeasureIndex = mE.Index;
			CollaboratorIndex = cLn.Index;
			var cLr = CollaborationManager.GetSpecifiedCollaborator(CollaboratorIndex);
			MeasureDuration = MeasureManager.GetMeasureDuration(mE, cLr);
			this.IsPackedMeasure = MeasureManager.GetPackState(mE, cLr);
			this.IsInPackedMeasuregroup = MeasuregroupManager.GetPackState(mE, cLr);
        }
    }
}
