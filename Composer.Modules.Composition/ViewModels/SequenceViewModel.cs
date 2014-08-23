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
        public int SequenceIndex { get; set; }

	    public SequenceViewModel(string sQ)
	    {
	        this.SequenceIndex = int.Parse(sQ);
            SubscribeEvents();
	    }

        public Chord LastCh { get; set; }

        private List<Chord> _activeChs;
        public List<Chord> ActiveChs
        {
            get { return this._activeChs ?? (this._activeChs = new List<Chord>()); }
            set
            {
                this._activeChs = value;
                this._activeChs = new List<Chord>(this._activeChs.OrderBy(p => p.Location_X));
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
			int? sQiX = payload.Item3;
			if (sQiX == null) return;
			var scope = payload.Item4;
			if (IsTargetVM(sQiX, scope))
			{
				this.ActiveChs = Utils.GetActiveChordsBySequence((int)sQiX, Guid.Empty);
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
			int sQiDx = payload.Item3;
			double? wI = payload.Item2;
			if (!IsTargetVM(sQiDx)) return;
			var sQ = SequenceManager.GetSequence(sQiDx);
			foreach (var mG in sQ.Measuregroups)
			{
				if (LastCh.Location_X + Preferences.M_END_SPC > Preferences.CompositionMeasureWidth)
				{
					if (wI == null)
					{
						wI = LastCh.Location_X + Preferences.M_END_SPC;
					}
					payload = new Tuple<Guid, double?, int>(mG.Id, wI, sQiDx);
					EA.GetEvent<BumpMeasuregroupWidth>().Publish(payload);
				}
			}
		}

        public void DefineCommands()
        {
            throw new NotImplementedException();
        }

        public bool IsTargetVM(int sQiDx)
        {
            return this.SequenceIndex == sQiDx;
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }

		public bool IsTargetVM(int? sQiX, _Enum.Scope scope)
		{
			return this.SequenceIndex == sQiX && (scope == _Enum.Scope.All || scope == _Enum.Scope.Sequence);
		}
    }
}