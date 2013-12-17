using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition
{
    public static class Utils
    {
        public static IEnumerable<Staffgroup> GetStaffgroups()
        {
            return CompositionManager.Composition.Staffgroups;
        }

        public static Staffgroup GetStaffgroup(Guid id)
        {
            var sg = (from a in Cache.Staffgroups where a.Id == id select a).SingleOrDefault();
            return sg;
        }

        public static Staffgroup GetStaffgroup(Measure m)
        {
            var s = GetStaff(m.Staff_Id);
            var sg = (from a in Cache.Staffgroups where a.Id == s.Staffgroup_Id select a).SingleOrDefault();
            return sg;
        }

        public static IEnumerable<Staff> GetStaffs(Guid sgId)
        {
            var ss = (from a in Cache.Staffs where a.Staffgroup_Id == sgId select a).ToList();
            return ss;
        }

        public static Staff GetStaff(Guid id)
        {
            var s = (from a in Cache.Staffs where a.Id == id select a).SingleOrDefault();
            return s;
        }

        public static IEnumerable<Measure> GetMeasures(Staff s)
        {
            var ms = (from a in Cache.Measures where a.Staff_Id == s.Id select a).ToList();
            return ms;
        }

        public static IEnumerable<Measure> GetMeasures(Guid sgId)
        {
            var measures = new List<Measure>();
            var ss = GetStaffs(sgId);
            foreach (var ms in ss.Select(s => GetMeasures(s)))
            {
                measures.AddRange(ms);
            }
            return measures;
        }

        public static IEnumerable<Measure> GetMeasuresInSequence(int sequence)
        {
            return (from a in Cache.Measures where a.Sequence == sequence select a).ToList();
        }

        public static IEnumerable<Measure> GetPackedMeasuresInSequence(int sequence)
        {
            return (from a in GetMeasuresInSequence(sequence) where MeasureManager.IsPacked(a) select a).ToList();
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

        public static Chord GetChord(Guid id)
        {
            var ch = (from a in Cache.Chords where a.Id == id select a).SingleOrDefault();
            return ch;
        }

        public static Chord GetChordWithStarttime(Guid mId, double? st)
        {
            if (st == null) return null;
            var m = GetMeasure(mId);
            return (from a in ChordManager.GetActiveChords(m.Chords) where a.StartTime == st select a).FirstOrDefault();
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

        public static int GetMaxMeasureWidthInSequence(int sequence)
        {
            return (int)((from a in GetMeasuresInSequence(sequence) select double.Parse(a.Width)).Max());
        }

        public static Measure GetMeasureWithMostChordsInSequence(int sequence)
        {
            var m =
                (from a in GetMeasuresInSequence(sequence) select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }

        public static Measure GetMeasureWithMostChordsInSequence(int sequence, Guid mId)
        {
            var m =
                (from a in GetMeasuresInSequence(sequence) where a.Id != mId select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }
    }
}
