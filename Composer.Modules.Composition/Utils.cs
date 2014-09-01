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
        public static Staffgroup GetStaffgroup(Guid sGId)
        {
            var sg = (from a in Cache.Staffgroups where a.Id == sGId select a).SingleOrDefault();
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

        /// <summary>
        /// Returns all measures in a measure group. A measure group is all measures in a staffgroup with the same sequence.
        /// </summary>
        /// <param name="measure"></param>
        /// <returns></returns>
        public static IEnumerable<Measure> GetMeasureGroup(Measure measure)
        {
            var sG = GetStaffgroup(measure);
            var sFs = GetStaffs(sG.Id);
            return (from s in sFs from m in s.Measures where m.Sequence == measure.Sequence select m).OrderBy(s => s.Index).ToList();
        }

        public static IEnumerable<Measure> GetMeasureGroup(List<Staff> sFs, int sQ)
        {
            return (from s in sFs from m in s.Measures where m.Sequence == sQ select m).OrderBy(s => s.Index).ToList();
        }

        public static IEnumerable<Chord> GetMeasureGroupChords(Measure measure, Guid excludeMId, _Enum.SortOrder sortOrder, _Enum.Filter filter = _Enum.Filter.Distinct)
        {
            var cHs = new List<Chord>();
            var sTs = new List<double>();
	        //if (measure == null) return null;
	        if (measure == null)
	        {
		        
	        }
            IEnumerable<Measure> mS = GetMeasureGroup(measure);
            foreach (var m in mS)
            {
                if (m.Id == excludeMId && filter == _Enum.Filter.Distinct) continue;
                var activeChs = ChordManager.GetActiveChords(m.Chords);
                foreach (var activeCh in activeChs)
                {
                    if (activeCh.StartTime != null)
                    {
                        var sT = (double)activeCh.StartTime;
                        if (sTs.Contains(sT)) continue;
                        if (filter == _Enum.Filter.Distinct) sTs.Add(sT);
                        cHs.Add(activeCh);
                    }
                }
            }
            return (sortOrder == _Enum.SortOrder.Ascending) ? cHs.OrderBy(p => p.StartTime) : cHs.OrderByDescending(p => p.StartTime);
        }

        public static IEnumerable<Chord> GetMeasureGroupChords(Guid mId, Guid excludeMId, _Enum.Filter filter = _Enum.Filter.Distinct)
        {
            return GetMeasureGroupChords(GetMeasure(mId), excludeMId, _Enum.SortOrder.Descending, filter);
        }

        public static List<Chord> GetActiveChordsBySequence(int seq, Guid excludeMId)
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
            var d = chs.OrderBy(p => p.Location_X).ToList();
	        return d;
        }

        public static IEnumerable<Measure> GetMeasuresBySequence(int seq)
        {
            return (from a in Cache.Measures where a.Sequence == seq select a).ToList();
        }

		public static Measure GetMeasure(Guid iD)
        {
            var m = (from a in Cache.Measures where a.Id == iD select a).SingleOrDefault();
            return m;
        }

        public static Measure GetMeasure(int iX)
        {
            var mE = (from a in Cache.Measures where a.Index == iX select a).SingleOrDefault();
            return mE;
        }

        public static Measure GetMeasure(Note nT)
        {
            var cH = GetChord(nT.Chord_Id);
            var mE = (from a in Cache.Measures where a.Id == cH.Measure_Id select a).SingleOrDefault();
            return mE;
        }

		public static Chord GetChord(Note nT)
		{
			return GetChord(nT.Chord_Id);
		}

        public static Chord GetChord(Guid id)
        {
            var cH = (from a in Cache.Chords where a.Id == id select a).SingleOrDefault();
            return cH;
        }

        public static Note GetNote(Guid iD)
        {
            var nT = (from a in Cache.Notes where a.Id == iD select a).First();
            return nT;
        }

        public static int GetStaffgroupSequenceById(Guid iD)
        {
            var sequence = (from a in Cache.Staffgroups where a.Id == iD select a.Sequence).SingleOrDefault();
            return sequence;
        }
    }
}
