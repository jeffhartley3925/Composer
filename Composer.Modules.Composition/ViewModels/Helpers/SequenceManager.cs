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
            Sequencegroup qG;
            CompSqs = new List<Sequencegroup>();
            foreach (var sG in Cache.Staffgroups)
            {
                foreach (var sF in sG.Staffs.OrderBy(j => j.Index))
                {
                    foreach (var mE in sF.Measures.OrderBy(j => j.Index))
                    {
                        qG = new Sequencegroup(mE.Sequence);
                        qG.Measures = Utils.GetMeasuresBySequence(mE.Sequence).ToList();
                        CompSqs.Add(qG);
                    }
                    break;
                }
                break;
            }
	        Update();
        }

		public static Sequencegroup GetSequence(int sQ)
		{
			return (from a in CompSqs where a.Sequence == sQ select a).First();
		}

		public static void Update()
		{
			Ea.GetEvent<UpdateSequences>().Publish(CompSqs);
		}
	}
}