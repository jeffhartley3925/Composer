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
            MeasureStatistics.RemoveAll(x => x.MeasureId == mId);
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
        public int? MeasureIndex { get; set; }
        public int CollaboratorIndex { get; set; }
        public bool IsPacked { get; set; }
        public bool IsFull { get; set; }

        private static readonly Tuple<bool, int?, double, bool> NullPackState = new Tuple<bool, int?, double, bool>(false, null, 0, false);

        public CollaborationStatistics(Repository.DataService.Measure m, Repository.DataService.Collaboration collaborator)
        {
            MeasureId = m.Id;
            MeasureIndex = m.Index;
            CollaboratorIndex = collaborator.Index;
            var mPackState =  GetPackedState(m, collaborator.Index);
            MeasureDuration = mPackState.Item3;
            // IsPacked and IsFull measure the same thing when the staffgroup measure collection contains only one measure.
            IsPacked = mPackState.Item1; // if true, one or more measures in the staffgroup measure collection IsFull.
            IsFull = mPackState.Item4; // if true, the referenced measure IsFull
        }

        private static Tuple<bool, int?, double, bool> GetPackedState(Repository.DataService.Measure m, int collaboratorIndex)
        {
            if (m == null) return NullPackState;
            if (!m.Chords.Any()) return NullPackState;
            if (m.Chords.Count == 0) return NullPackState;
            var collaborator = CollaborationManager.GetSpecifiedCollaborator(collaboratorIndex);
            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument)
            {
                return IsPackedStaffMeasure(m, collaborator);
            }
            return IsPackedStaffgroupMeasure(m, collaborator);
        }

        public static Tuple<bool, int?, double, bool> IsPackedStaffMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            var d = Convert.ToDouble((from ch in m.Chords where CollaborationManager.IsActive(ch, collaborator) select ch.Duration).Sum());
            return new Tuple<bool, int?, double, bool>(d >= DurationManager.Bpm, m.Index, d, false);
        }

        public static Tuple<bool, int?, double, bool> IsPackedStaffgroupMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            // this method returns meaningful results iff the staff density is 2.
            // in other words this function returns meaningful results iff the staff configuration is 'Grand.'
            // TODO: extend this method so that it works when the staff density is > 2. Easy.

            var mPackState = IsPackedStaffMeasure(m, collaborator);
            var mDuration = mPackState.Item3;
            if (mPackState.Item1) return mPackState;

            var mStaff = Utils.GetStaff(m.Staff_Id);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            m = Utils.GetMeasure(mIndex);

            mPackState = IsPackedStaffMeasure(m, collaborator);
            if (mPackState.Item3 > mDuration)
            {
                mDuration = mPackState.Item3;
            }
            return new Tuple<bool, int?, double, bool>(mDuration >= DurationManager.Bpm, m.Index, mDuration, false);
        }
    }
}
