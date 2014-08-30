using System.Linq;

namespace Composer.Modules.Composition.ViewModels
{
	using System;
	using System.Collections.Generic;

	using Composer.Infrastructure;
	using Composer.Infrastructure.Events;
	using Composer.Modules.Composition.ViewModels.Helpers;
	using Composer.Repository.DataService;

	public class SequenceViewModel : BaseViewModel, ISequenceViewModel, IEventCatcher
    {
        public int Sequence { get; set; }

	    public SequenceViewModel(string sQ)
	    {
	        this.Sequence = int.Parse(sQ);
            SubscribeEvents();
	    }

        public Chord LastCh { get; set; }

        private List<Chord> activeChs;
        public List<Chord> ActiveChs
        {
            get { return this.activeChs ?? (this.activeChs = new List<Chord>()); }
            set
            {
                this.activeChs = value;
                this.activeChs = new List<Chord>(this.activeChs.OrderBy(p => p.Location_X));
            }
        }

        public void OnResizeSequence(object obj)
        {
            var payload = (WidthChange) obj;
	        if (payload.Sequence == null || !this.IsTargetVM((int)payload.Sequence)) return;
			var sQ = SequenceManager.GetSequence((int)payload.Sequence);
			foreach (var mG in sQ.Measuregroups)
			{
				payload.MeasuregroupId = mG.Id;
				EA.GetEvent<ResizeMeasuregroup>().Publish(payload);
			}
        }

		public void OnRespaceSequence(Tuple<Guid, int?> payload)
		{
			if (payload.Item2 == null) return;
			var sQiDx = (int)payload.Item2;
			if (!this.IsTargetVM(sQiDx)) return;
			var sQ = SequenceManager.GetSequence(sQiDx);
			foreach (var mG in sQ.Measuregroups)
			{
				EA.GetEvent<RespaceMeasuregroup>().Publish(mG.Id);
			}
		}

		public void OnUpdateActiveChords(Tuple<Guid, Guid, int?, _Enum.Scope> payload)
		{
			int? sQ = payload.Item3;
			if (sQ == null) return;
			var scope = payload.Item4;
			if (IsTargetVM(sQ, scope))
			{
				this.ActiveChs = Utils.GetActiveChordsBySequence((int)sQ, Guid.Empty);
				if (ActiveChs.Any())
				{
					this.LastCh = (from c in this.ActiveChs select c).Last();
				}
			}
		}

        public void SubscribeEvents()
        {
            EA.GetEvent<ResizeSequence>().Subscribe(OnResizeSequence);
			EA.GetEvent<RespaceSequence>().Subscribe(OnRespaceSequence);
			EA.GetEvent<UpdateActiveChords>().Subscribe(OnUpdateActiveChords);
			EA.GetEvent<BumpSequenceWidth>().Subscribe(OnBumpSequenceWidth);
        }

		public void OnBumpSequenceWidth(Tuple<Guid, double?, int> payload)
		{
			int sQiX = payload.Item3;
			double? wI = payload.Item2;
			if (!IsTargetVM(sQiX)) return;
			var sQ = SequenceManager.GetSequence(sQiX);
			if (LastCh == null) return;
			foreach (var mG in sQ.Measuregroups)
			{
				if (LastCh.Location_X + Preferences.M_END_SPC > Preferences.CompositionMeasureWidth)
				{
					if (wI == null)
					{
						wI = LastCh.Location_X + Preferences.M_END_SPC;
					}
					payload = new Tuple<Guid, double?, int>(mG.Id, wI, sQiX);
					EA.GetEvent<BumpMeasuregroupWidth>().Publish(payload);
				}
			}
		}

        public void DefineCommands()
        {
            throw new NotImplementedException();
        }

        public bool IsTargetVM(int sQ)
        {
            return this.Sequence == sQ;
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }

		public bool IsTargetVM(int? sQ, _Enum.Scope scope)
		{
			return this.Sequence == sQ && (scope == _Enum.Scope.All || scope == _Enum.Scope.Sequence);
		}
    }
}