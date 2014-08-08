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
        }

        public static void Spinup()
        {
	        var iX = 0;
            CompMgs = new List<Measuregroup>();
            foreach (var sG in Cache.Staffgroups)
            {
                foreach (var sF in sG.Staffs.OrderBy(j => j.Index))
                {
                    foreach (var mE in sF.Measures.OrderBy(j => j.Index))
                    {
                        CompMgs.Add(CreateMeasureGroup(sG, mE, iX++));
                    }
                    break;
                }
            }
            Ea.GetEvent<UpdateMeasuregroups>().Publish(CompMgs);
        }

        private static Measuregroup CreateMeasureGroup(Staffgroup sG, Measure mE, int index)
        {
            var mG = new Measuregroup(sG.Id, mE.Sequence, index);
            mG.Measures = Utils.GetMeasureGroup(sG.Staffs.ToList(), mE.Sequence);
            var parentSequence = (from a in SequenceManager.CompSqs where a.SequenceIndex == mE.Sequence select a).FirstOrDefault();
            mG.Sequence = parentSequence;
            parentSequence.Measuregroups.Add(mG);
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

		public static PackState GetPackState(Measure mE, Collaborator cL)
		{
			var mG = GetMeasuregroup(mE.Id, true);
			if (mG == null) return new PackState(false, false);
			foreach (var m in mG.Measures)
			{
				var pS = MeasureManager.GetPackState(m, cL);
				if (pS.PackedMeasure)
				{
					return pS;
				}
			}
			return new PackState(false, false);
		}

		public static bool IsPacked(Measure m)
		{
			bool result = (Statistics.MeasureStatistics.Where(
				b => b.MeasureId == m.Id && b.CollaboratorIndex == Collaborations.Index).Select(b => b.IsPackedMeasuregroup)).First();
			return result;
		}
    }
}
