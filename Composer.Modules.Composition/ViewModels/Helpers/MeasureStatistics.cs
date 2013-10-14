using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class Statistics
    {
        public static List<MeasureStatistic> MeasureStatistics = null;

        public static void Add(Repository.DataService.Measure m)
        {
            if (MeasureStatistics == null)
            {
                MeasureStatistics = new List<MeasureStatistic>();
            }
            foreach (var col in CompositionManager.Composition.Collaborations)
            {
                MeasureStatistics.Add(new MeasureStatistic(m, col));
            }
        }

        public static void Remove(Guid id, int index)
        {
            var a = (from b in MeasureStatistics where b.CollaboratorIndex == index && b.MeasureId == id select b);
            foreach (var statistic in a)
            {
                MeasureStatistics.Remove(statistic);
                break;
            }
        }
    }

    public class MeasureStatistic
    {
        public Guid Id { get; set; }
        public Guid MeasureId { get; set; }
        public int CollaboratorIndex { get; set; }
        public bool Packed { get; set; }

        public MeasureStatistic(Repository.DataService.Measure m, Repository.DataService.Collaboration col)
        {
            Id = Guid.NewGuid();
            MeasureId = m.Id;
            CollaboratorIndex = col.Index;
            Packed = GetPackedState(m);
        }

        private bool GetPackedState(Repository.DataService.Measure m)
        {
            var collaborator = CollaborationManager.GetSpecifiedCollaborator(CollaboratorIndex);
            var isPacked = IsPackedStaffMeasure(m, collaborator);
            if (isPacked) return true;
            //isPacked = MeasureManager.IsPackedStaffgroupMeasure(m, collaborator);
            return isPacked;
        }

        public static bool IsPackedStaffMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            if (m == null) return false;
            if (!m.Chords.Any()) return false;
            //var chs = ChordManager.GetActiveChords(m, collaborator);
            var chs = m.Chords;
            if (chs.Count <= 0) return false;
            var mDuration = Convert.ToDouble((from c in chs where CollaborationManager.IsActive(c, collaborator) select c.Duration).Sum());
            return mDuration >= DurationManager.Bpm;
        }

        public static bool IsPackedStaffgroupMeasure(Repository.DataService.Measure m, Collaborator collaborator)
        {
            if (m == null) return false;

            var packed = IsPackedStaffMeasure(m, collaborator);
            if (packed) return true;

            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument) return false;

            var mStaff = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            m = (from a in Cache.Measures where a.Index == mIndex select a).First();
            return IsPackedStaffMeasure(m, collaborator);
        }
    }
}
