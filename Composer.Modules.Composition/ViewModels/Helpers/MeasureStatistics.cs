using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    // depends on CompositionManager.Composition
    // depends on DurationManager.Bpm

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
            var mDuration = Convert.ToDouble((from c in m.Chords 
                                              where CollaborationManager.IsActive(c, collaborator) 
                                              select c.Duration).Sum());
            return mDuration >= DurationManager.Bpm;
        }
    }
}
