using System;
using System.Linq;
using System.Collections.Generic;
using Composer.Infrastructure;
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

        public static Staffgroup GetStaffgroup(Guid sGId)
        {
            var sg = (from a in Cache.Staffgroups where a.Id == sGId select a).SingleOrDefault();
            return sg;
        }

        public static Staffgroup GetStaffgroup(Guid mId, bool overload)
        {
            var m = GetMeasure(mId);
            return GetStaffgroup(m);
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

        public static IEnumerable<Chord> GetSequenceChords(Measure m, Guid excludeMId)
        {
            var ms = GetMeasuresBySequence(m.Sequence);
            return (from n in ms from ch in ChordManager.GetActiveChords(n.Chords) where n.Id != excludeMId select ch).OrderByDescending(v => v.StartTime).ToList();
        }

        /// <summary>
        /// Returns all measures in a measure group. A measure group is all measures in a staffgroup with the same seq.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static IEnumerable<Measure> GetMeasureGroup(Measure m)
        {
            var sg = GetStaffgroup(m);
            var sGs = GetStaffs(sg.Id);
            return (from s in sGs from n in s.Measures where n.Sequence == m.Sequence select n).ToList();
        }

        public static IEnumerable<Chord> GetMeasureGroupChords(Guid mId, Guid excludeMId, bool distinct = true)
        {
            return GetMeasureGroupChords(GetMeasure(mId), excludeMId, _Enum.SortOrder.Descending, distinct);
        }

        public static IEnumerable<Chord> GetMeasureGroupChords(Measure measure, Guid excludeMId, _Enum.SortOrder sortOrder, bool distinct = true)
        {
            var chs = new List<Chord>();
            var starttimes = new List<double>();
            IEnumerable<Measure> ms = GetMeasureGroup(measure);
            foreach (var n in ms)
            {
                if (n.Id == excludeMId) continue;
                var activeChords = ChordManager.GetActiveChords(n.Chords);
                foreach (var activeChord in activeChords)
                {
                    if (activeChord.StartTime != null)
                    {
                        var st = (double)activeChord.StartTime;
                        if (!starttimes.Contains(st))
                        {
                            if (distinct) starttimes.Add(st);
                            chs.Add(activeChord);
                        }
                    }
                }
            }
            return (sortOrder == _Enum.SortOrder.Ascending) ? chs.OrderBy(p => p.StartTime) : chs.OrderByDescending(p => p.StartTime);
        }

        public static IEnumerable<Chord> GetActiveChordsBySequence(int seq, Guid excludeMId)
        {
            var chs = new List<Chord>();
            var measures = GetMeasuresBySequence(seq);
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

        public static IEnumerable<Measure> GetMeasuresBySequence(int seq, Guid excludedMId)
        {
            return (from a in Cache.Measures where a.Sequence == seq && a.Id != excludedMId select a).ToList();
        }

        public static IEnumerable<Measure> GetMeasuresBySequence(Guid mId)
        {
            var m = GetMeasure(mId);
            var sequence = m.Sequence;
            return (from a in Cache.Measures.OrderByDescending(v => v.Chords.Count()) where a.Sequence == sequence && a.Chords.Count > 0 select a).ToList();
        }

        public static IEnumerable<Measure> GetMeasuresBySequenceOrderedByCalculation(Guid mId)
        {
            var m = GetMeasure(mId);
            var sequence = m.Sequence;
            return (from a in Cache.Measures.OrderByDescending(v => Calculation(v.Chords, v.Index)) where a.Sequence == sequence && a.Chords.Count > 0 select a).ToList();
        }

        private static double Calculation(IEnumerable<Chord> chs, int mDx)
        {
            double result = 0;
            var e = chs as Chord[] ?? chs.ToArray();
            if (e.Any())
            {
                double totalChDur = e.Sum(ch => (double)ch.Duration);
                double avgChDur = totalChDur / e.Count();
                result = e.Count() / avgChDur;
            }
            return result;
        }

        public static IEnumerable<Measure> GetMeasuresBySequence(int seq)
        {
            return (from a in Cache.Measures where a.Sequence == seq select a).ToList();
        }

        public static IEnumerable<Measure> GetPackedMeasuresBySequence(int seq)
        {
            return (from a in GetMeasuresBySequence(seq) where MeasureManager.IsPackedForStaff(a) select a).ToList();
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

        public static int GetStaffgroupSequenceById(Guid id)
        {
            var sequence = (from a in Cache.Staffgroups where a.Id == id select a.Sequence).SingleOrDefault();
            return sequence;
        }

        public static int GetMaxMeasureWidthBySequence(int seq)
        {
            return (int)((from a in GetMeasuresBySequence(seq) select double.Parse(a.Width)).Max());
        }

        public static Measure GetMeasureWithMaxChordCountBySequence(int seq, Guid mId)
        {
            var m =
                (from a in GetMeasuresBySequence(seq, mId) select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }

        public static Measure GetMeasureWithMaxChordCountBySequence(int seq)
        {
            var m =
                (from a in GetMeasuresBySequence(seq) select a).OrderByDescending(
                    b => ChordManager.GetActiveChords(b.Chords).Count).FirstOrDefault();
            return m;
        }

        public static Measure GetMeasureWithMaxChordStarttimeBySequence(int seq)
        {
            double? maxSt = 0;
            Measure maxStM = null;
            foreach (var m in GetMeasuresBySequence(seq))
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="mId"></param>
        /// <param name="excludeChId"></param>
        /// <returns></returns>
        public static Chord GetRightmostChInMg(int seq, Guid mId, Guid excludeChId)
        {
            double? maxSt = 0;
            Chord maxStCh = null;
            var measure = GetMeasure(mId);
            IEnumerable<Measure> mG = GetMeasureGroup(measure);
            foreach (var m in mG)
            {
                if (m.Id == mId) continue;
                var ch = (from b in ChordManager.GetActiveChords(m.Chords).OrderByDescending(v => v.StartTime) select b).FirstOrDefault();
                if (ch == null) continue;
                if (ch.Id == excludeChId) continue;
                if (ch.StartTime > maxSt)
                {
                    maxStCh = ch;
                    maxSt = ch.StartTime;
                }
            }
            return maxStCh;
        }
    }
}
