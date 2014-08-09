using System;
using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
	public static class MeasuregroupManager
    {
        private static readonly IEventAggregator Ea;
        public static List<Measuregroup> CompMgs = null;

        static MeasuregroupManager()
        {
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
			SubscribeEvents();
        }

		private static void SubscribeEvents()
		{

		}

		public static void Spinup()
        {
	        var mGiX = 0;
            CompMgs = new List<Measuregroup>();
            foreach (var sG in Cache.Staffgroups)
            {
                foreach (var sF in sG.Staffs.OrderBy(j => j.Index))
                {
                    foreach (var mE in sF.Measures.OrderBy(j => j.Index))
                    {
                        CompMgs.Add(CreateMeasureGroup(sG, mE, mGiX++));
                    }
                    break;
                }
            }
            Ea.GetEvent<UpdateMeasuregroups>().Publish(CompMgs);
        }

        private static Measuregroup CreateMeasureGroup(Staffgroup sG, Measure mE, int mGiX)
        {
            var mG = new Measuregroup(sG.Id, mE.Sequence, mGiX);
            mG.Measures = Utils.GetMeasureGroup(sG.Staffs.ToList(), mE.Sequence);
            var sequence = (from a in SequenceManager.CompSqs where a.SequenceIndex == mE.Sequence select a).FirstOrDefault();
            mG.Sequence = sequence;
            sequence.Measuregroups.Add(mG);
            return mG;
        }

        public static Measuregroup GetMeasuregroup(Guid mGiD)
        {
			if (CompMgs == null) return null;
            return (from a in CompMgs where a.Id == mGiD select a).First();
        }

		public static Measuregroup GetMeasuregroup(Guid mEiD, bool overload)
		{
			if (CompMgs == null) return null;
			var b = (from a in CompMgs where a.Measures.Contains(Utils.GetMeasure(mEiD)) select a);
			if (b.Any())
			{
				return b.First();
			}
			return null;
		}

		public static bool GetPackState(Measure mE, Collaborator cL)
		{
			var mG = GetMeasuregroup(mE.Id, true);
			if (mG == null) return false;
			foreach (var m in mG.Measures)
			{
				var isPacked = MeasureManager.GetPackState(m, cL);
				if (isPacked) return true;
			}
			return false;
		}

		public static bool IsPacked(Measure mE)
		{
			return IsPacked(mE, Collaborations.Index);
		}

		public static bool IsPacked(Measure mE, int cLrIx)
		{
			if (mE.Chords.Count == 0) return false;
			var a =
				(Statistics.CompositionMeasureStatistics.Where(b => b.MeasureId == mE.Id && b.CollaboratorIndex == cLrIx)
					.Select(b => b.IsInPackedMeasuregroup));
			return a.Any() ? a.First() : false;
		}
    }
}
