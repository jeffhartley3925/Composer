using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
	public static class SequenceManager
	{
		private static readonly IEventAggregator Ea;
        public static List<Sequencegroup> CompSqs;

		static SequenceManager()
		{
			Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
		}

        /// <summary>
        /// Initializes the in-memory collection of Sequence entities</summary>
        public static void Spinup()
        {
            Sequencegroup cSq;
            CompSqs = new List<Sequencegroup>();
            foreach (var sG in Cache.Staffgroups)
            {
                foreach (var sF in sG.Staffs.OrderBy(j => j.Index))
                {
                    foreach (var mE in sF.Measures.OrderBy(j => j.Index))
                    {
                        cSq = new Sequencegroup(mE.Sequence);
                        cSq.Measures = Utils.GetMeasuresBySequence(mE.Sequence).ToList();
                        CompSqs.Add(cSq);
                    }
                    break;
                }
                break;
            }
	        Update();
        }

		public static Sequencegroup GetSequence(int sQiX)
		{
			return (from a in CompSqs where a.SequenceIndex == sQiX select a).First();
		}

		public static void Update()
		{
			Ea.GetEvent<UpdateSequences>().Publish(CompSqs);
		}
	}
}