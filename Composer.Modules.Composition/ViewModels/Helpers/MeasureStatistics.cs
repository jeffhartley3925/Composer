using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
	public static class Statistics
    {
        public static List<CollaborationStatistics> MeasureStatistics = null;

        public static void Add(Repository.DataService.Measure mE)
        {
            if (MeasureStatistics == null)
            {
                MeasureStatistics = new List<CollaborationStatistics>();
            }
            Remove(mE.Id);
            foreach (var cL in CompositionManager.Composition.Collaborations)
            {
                MeasureStatistics.Add(new CollaborationStatistics(mE, cL));
            }
        }

        public static void Remove(Guid mEiD)
        {
			MeasureStatistics.RemoveAll(x => x.MeasureId == mEiD);
        }

		public static void Update(Guid mEiD)
        {
            var mE = Utils.GetMeasure(mEiD);
            Remove(mEiD);
            Add(mE);
        }
    }

    public class CollaborationStatistics
    {
        public Guid MeasureId { get; set; }
        public int? MeasureIndex { get; set; }
        public int CollaboratorIndex { get; set; }
        public bool IsPackedMeasure { get; set; }
        public bool IsPackedMeasuregroup { get; set; }
       
		private static PackState NullPackState = new PackState(false, false);

        public CollaborationStatistics(Repository.DataService.Measure mE, Repository.DataService.Collaboration cL)
        {
            MeasureId = mE.Id;
            MeasureIndex = mE.Index;
            CollaboratorIndex = cL.Index;
            var pS =  GetPackState(mE, cL.Index);
	        this.IsPackedMeasure = pS.PackedMeasure;
            this.IsPackedMeasuregroup = pS.PackedMeasuregroup;
        }

		private static PackState GetPackState(Repository.DataService.Measure mE, int cLiX)
        {

            if (mE == null) return NullPackState;
            if (!mE.Chords.Any()) return NullPackState;
            if (mE.Chords.Count == 0) return NullPackState;
            var cL = CollaborationManager.GetSpecifiedCollaborator(cLiX);
            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument)
            {
                return MeasureManager.GetPackState(mE, cL);
            }
            return MeasuregroupManager.GetPackState(mE, cL);
        }
    }
}
