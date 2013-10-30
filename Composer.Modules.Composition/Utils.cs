using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition
{
    public static class Utils
    {
        public static IEnumerable<Staffgroup> GetCompositionStaffgroups()
        {
            return CompositionManager.Composition.Staffgroups;
        }

        public static IEnumerable<Staff> GetStaffgroupStaffs(Guid sgId)
        {
            var ss = (from a in Cache.Staffs where a.Staffgroup_Id == sgId select a).ToList();
            return ss;
        }

        public static Measure GetMeasure(Guid id)
        {
            var m = (from a in Cache.Measures where a.Id == id select a).SingleOrDefault();
            return m;
        }

        public static Measure GetMeasure(int index)
        {
            var m = (from a in Cache.Measures where a.Index == index select a).SingleOrDefault();
            return m;
        }

        public static Measure GetMeasure(Note n)
        {
            var ch = GetChord(n.Chord_Id);
            var m = (from a in Cache.Measures where a.Id == ch.Measure_Id select a).SingleOrDefault();
            return m;
        }

        public static Staff GetStaff(Guid id)
        {
            var s = (from a in Cache.Staffs where a.Id == id select a).SingleOrDefault();
            return s;
        }

        public static Staffgroup GetStaffgroup(Guid id)
        {
            var sg = (from a in Cache.Staffgroups where a.Id == id select a).SingleOrDefault();
            return sg;
        }

        public static Chord GetChord(Guid id)
        {
            var ch = (from a in Cache.Chords where a.Id == id select a).SingleOrDefault();
            return ch;
        }

        public static Note GetNote(Guid id)
        {
            var n = (from a in Cache.Notes where a.Id == id select a).SingleOrDefault();
            return n;
        }

        public static int GetStaffgroupSequence(Guid id)
        {
            var sequence = (from a in Cache.Staffgroups where a.Id == id select a.Sequence).SingleOrDefault();
            return sequence;
        }
    }
}
