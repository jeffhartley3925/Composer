using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
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

        public static IEnumerable<Chord> GetActiveChordsBySequence(int sequence, Guid excludeMId)
        {
            var chs = new List<Chord>();
            var measures = GetMeasuresBySequence(sequence);
            foreach (var m in measures)
            {
                if (m.Id == excludeMId || m.Chords.Count <= 0) continue;
                var activeChords = ChordManager.GetActiveChords(m.Chords);
                if (activeChords.Count > 0)
                {
                    chs.AddRange(activeChords);
                }
            }
            return chs.OrderBy(p => p.StartTime);
        }

        public static IEnumerable<Measure> GetMeasuresBySequence(int sequence, Guid mId)
        {
            return (from a in Cache.Measures where a.Sequence == sequence && a.Id != mId select a).ToList();
        }

        public static IEnumerable<Measure> GetMeasuresBySequence(int sequence)
        {
            return (from a in Cache.Measures where a.Sequence == sequence select a).ToList();
        }

        public static IEnumerable<Measure> GetPackedMeasuresBySequence(int sequence)
        {
            return (from a in GetMeasuresBySequence(sequence) where MeasureManager.IsPackedForStaff(a) select a).ToList();
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

        public static Note GetNote(Guid id)
        {
            var n = (from a in Cache.Notes where a.Id == id select a).SingleOrDefault();
            return n;
        }

        public static int GetStaffgroupBySequence(Guid id)
        {
            var sequence = (from a in Cache.Staffgroups where a.Id == id select a.Sequence).SingleOrDefault();
            return sequence;
        }

        public static int GetMaxMeasureWidthBySequence(int sequence)
        {
            return (int)((from a in GetMeasuresBySequence(sequence) select double.Parse(a.Width)).Max());
        }

        public static Measure GetMeasureWithMaxChordCountBySequence(int sequence, Guid mId)
        {
            var m =
                (from a in GetMeasuresBySequence(sequence, mId) select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }

        public static Measure GetMeasureWithMaxChordCountBySequence(int sequence)
        {
            var m =
                (from a in GetMeasuresBySequence(sequence) select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }

        public static Measure GetMeasureWithMaxChordStarttimeBySequence(int sequence)
        {
            double? maxSt = 0;
            Measure maxStM = null;
            foreach (var m in GetMeasuresBySequence(sequence))
            {
                var ch = (from b in ChordManager.GetActiveChords(m.Chords).OrderByDescending(v => v.StartTime) select b).FirstOrDefault();
                if (ch != null)
                {
                    if (!(ch.StartTime > maxSt)) continue;
                    maxSt = ch.StartTime;
                    maxStM = m;
                }
            }
            return maxStM;
        }

        public static Chord GetMaxStarttimeChordInSequence(int sequence, Guid mId, Guid excludeChId)
        {
            double? maxSt = 0;
            Chord maxStCh = null;
            foreach (var n in GetMeasuresBySequence(sequence))
            {
                if (n.Id == mId) continue;
                var ch = (from b in ChordManager.GetActiveChords(n.Chords).OrderByDescending(v => v.StartTime) select b).FirstOrDefault();
                if (ch == null) continue;
                if (ch.Id == excludeChId) continue;
                if (ch.StartTime > maxSt)
                {
                    maxStCh = ch;
                }
            }
            return maxStCh;
        }
    }
}
