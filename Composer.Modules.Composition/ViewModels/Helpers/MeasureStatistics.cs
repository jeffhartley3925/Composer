using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;

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
            Remove(m.Id);
            foreach (var collaborator in CompositionManager.Composition.Collaborations)
            {
                MeasureStatistics.Add(new CollaborationStatistics(m, collaborator));
            }
        }

        public static void Remove(Guid mId)
        {
            // remove all collaborationStatistics for a measure

            MeasureStatistics.RemoveAll(x => x.MeasureId == mId);

            //var statistics = new List<CollaborationStatistics>();
            //var a = (from b in MeasureStatistics where b.MeasureId == mId select b);
            //foreach (var statistic in a)
            //{
            //    MeasureStatistics.Remove(statistic);
            //}

            //var mStaff = Utils.GetStaff(m.Staff_Id);
            //var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            //var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            //m = Utils.GetMeasure(mIndex);
        }

        public static void Update(Repository.DataService.Measure m)
        {
            Remove(m.Id);
            Add(m);
        }

        public static void Update(Guid mId)
        {
            var m = Utils.GetMeasure(mId);
            Remove(mId);
            Add(m);
        }
    }

    public class CollaborationStatistics
    {
        public double MeasureDuration { get; set; }
        public Guid MeasureId { get; set; }
        public static int MeasureIndex { get; set; }
        public int CollaboratorIndex { get; set; }
        public bool IsPacked { get; set; }
        public bool IsFull { get; set; }

        public CollaborationStatistics(Repository.DataService.Measure m, Repository.DataService.Collaboration collaborator)
        {
            MeasureId = m.Id;
            MeasureIndex = m.Index;
            CollaboratorIndex = collaborator.Index;
            var mPackState =  GetPackedState(m);
            MeasureDuration = mPackState.Item3;
            IsPacked = mPackState.Item1;
            IsFull = mPackState.Item4;
        }

        private static readonly Tuple<bool, int, double, bool> NullPackState = new Tuple<bool, int, double, bool>(false, MeasureIndex, 0, false);

        private Tuple<bool, int, double, bool> GetPackedState(Repository.DataService.Measure m)
        {
            if (m == null) return NullPackState;
            if (!m.Chords.Any()) return NullPackState;
            if (m.Chords.Count == 0) return NullPackState;
            var collaborator = CollaborationManager.GetSpecifiedCollaborator(CollaboratorIndex);
            var mPackState = IsPackedStaffMeasure(m, collaborator);
            bool isFull = mPackState.Item1;
            if (mPackState.Item1) return new Tuple<bool, int, double, bool>(mPackState.Item1, m.Index, mPackState.Item3, isFull);
            Tuple<bool, int, double, bool> result;
            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument)
            {
                result = IsPackedStaffMeasure(m, collaborator);
                return result;
            }
            result = IsPackedStaffgroupMeasure(m, collaborator);
            return result;
        }

        public static Tuple<bool, int, double, bool> IsPackedStaffMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            var mDuration = Convert.ToDouble((from ch in m.Chords where CollaborationManager.IsActive(ch, collaborator) select ch.Duration).Sum());
            var result = new Tuple<bool, int, double, bool>(mDuration >= DurationManager.Bpm, m.Index, mDuration, false);
            return new Tuple<bool, int, double, bool>(mDuration >= DurationManager.Bpm, m.Index, mDuration, false);
        }

        public static Tuple<bool, int, double, bool> IsPackedStaffgroupMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            // this method returns meaningful results iff the staff density is 2.
            // in other words this function returns meaningful results iff the staff configuration is 'Grand.'
            // TODO: extend this method so that it works when the staff density is > 2. Easy.

            var mPackState = IsPackedStaffMeasure(m, collaborator);
            var mDuration = mPackState.Item3;
            bool isFull = mPackState.Item1;
            if (mPackState.Item1) return new Tuple<bool, int, double, bool>(mPackState.Item1, m.Index, mPackState.Item3, isFull);

            var mStaff = Utils.GetStaff(m.Staff_Id);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            m = Utils.GetMeasure(mIndex);
            mPackState = IsPackedStaffMeasure(m, collaborator);
            if (mPackState.Item3 > mDuration)
            {
                mDuration = mPackState.Item3;
            }
            var result = new Tuple<bool, int, double, bool>(mDuration >= DurationManager.Bpm, m.Index, mDuration, false);
            return new Tuple<bool, int, double, bool>(mDuration >= DurationManager.Bpm, m.Index, mDuration, false);
        }
    }
}
