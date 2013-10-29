using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class Statistics
    {
        public static List<CollaborationStatistics> MeasureStatistics = null;

        public static void Add(Repository.DataService.Measure m)
        {
            if (MeasureStatistics == null)
            {
                MeasureStatistics = new List<CollaborationStatistics>();
            }
            foreach (var col in CompositionManager.Composition.Collaborations)
            {
                MeasureStatistics.Add(new CollaborationStatistics(m, col));
            }
        }

        public static void Remove(Guid mId)
        {
            // remove all collaborationStatistics for a measure

            var a = (from b in MeasureStatistics where b.MeasureId == mId select b);
            foreach (var statistic in a)
            {
                MeasureStatistics.Remove(statistic);
                break;
            }
        }
    }

    public class CollaborationStatistics
    {
        public double MeasureDuration { get; set; }
        public Guid MeasureId { get; set; }
        public int MeasureIndex { get; set; }
        public int CollaboratorIndex { get; set; }
        public bool IsPacked { get; set; }

        public CollaborationStatistics(Repository.DataService.Measure m, Repository.DataService.Collaboration collaborator)
        {
            MeasureId = m.Id;
            MeasureIndex = m.Index;
            CollaboratorIndex = collaborator.Index;

           var mPackState =  GetPackedState(m);
            MeasureDuration = mPackState.Item2;
            IsPacked = mPackState.Item1;
        }

        private static readonly Tuple<bool, double> NullPackState = new Tuple<bool, double>(false, 0);

        private Tuple<bool, double> GetPackedState(Repository.DataService.Measure m)
        {
            if (m == null) return NullPackState;
            if (!m.Chords.Any()) return NullPackState;
            if (m.Chords.Count == 0) return NullPackState;
            var collaborator = CollaborationManager.GetSpecifiedCollaborator(CollaboratorIndex);
            var mPackState = IsPackedStaffMeasure(m, collaborator);
            if (mPackState.Item1) return new Tuple<bool, double>(mPackState.Item1, mPackState.Item2);

            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument) 
                return IsPackedStaffMeasure(m, collaborator);

            return IsPackedStaffgroupMeasure(m, collaborator);
        }

        public static Tuple<bool, double> IsPackedStaffMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            var mDuration = Convert.ToDouble((from ch in m.Chords where CollaborationManager.IsActive(ch, collaborator) select ch.Duration).Sum());
            return new Tuple<bool, double>(mDuration >= DurationManager.Bpm, mDuration);
        }

        public static Tuple<bool, double> IsPackedStaffgroupMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            // this method returns meaningful results iff the staff density is 2.
            // in other words this function returns meaningful results iff the staff configuration is 'Grand.'
            // TODO: extend this method so that it works when the staff density is > 2. Easy.

            var mPackState = IsPackedStaffMeasure(m, collaborator);
            var mDuration = mPackState.Item2;
            if (mPackState.Item1) return new Tuple<bool, double>(mPackState.Item1, mPackState.Item2);

            var mStaff = Utils.GetStaff(m.Staff_Id);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            m = Utils.GetMeasure(mIndex);
            mPackState = IsPackedStaffMeasure(m, collaborator);
            if (mPackState.Item2 > mDuration)
            {
                mDuration = mPackState.Item2;
            }
            return new Tuple<bool, double>(mDuration >= DurationManager.Bpm, mDuration);
        }
    }
}
