using System.Collections.ObjectModel;
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

        private IEnumerable<Chord> _activeChs;
        public IEnumerable<Chord> ActiveChs
        {
            get { return this._activeChs ?? (this._activeChs = new List<Chord>()); }
            set
            {
                this._activeChs = value;
                this._activeChs = new List<Chord>(this._activeChs.OrderBy(p => p.StartTime));
            }
        }

        public void OnResizeSequence(object obj)
        {
            var payload = (WidthChange) obj;
	        if (payload.Sequence == null || !this.IsTargetVM((int)payload.Sequence)) return;
			var sQ = SequenceManager.GetSequence((int)payload.Sequence);
			foreach (var mG in sQ.Measuregroups)
			{
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

		public void OnNotifyActiveChords(Tuple<Guid, object, object, object, int, Guid> payload)
        {
            var sQiDx = payload.Item5;
            if (!IsTargetVM(sQiDx)) return;
			this.LastCh = null;
            this.ActiveChs = (ObservableCollection<Chord>)payload.Item3;
			if (this.ActiveChs.Any())
			{
				this.LastCh = (from c in this.ActiveChs select c).Last();
			}
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<ResizeSequence>().Subscribe(OnResizeSequence);
			EA.GetEvent<RespaceSequence>().Subscribe(OnRespaceSequence);
			EA.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
			EA.GetEvent<BumpSequenceWidth>().Subscribe(OnBumpSequenceWidth);
        }

		public void OnBumpSequenceWidth(Tuple<Guid, double, int> payload)
		{
			int sQiDx = payload.Item3;
			if (!IsTargetVM(sQiDx)) return;
			var sQ = SequenceManager.GetSequence(sQiDx);
			foreach (var mG in sQ.Measuregroups)
			{
				if (LastCh.Location_X + Preferences.M_END_SPC > Preferences.CompositionMeasureWidth)
				{
					payload = new Tuple<Guid, double, int>(mG.Id, LastCh.Location_X + Preferences.M_END_SPC, payload.Item3);
					EA.GetEvent<BumpMeasuregroupWidth>().Publish(payload);
				}
			}
			//var mE = Utils.GetMeasure(payload.Item1);
			//EA.GetEvent<SetCompositionWidth>().Publish(mE.Staff_Id);
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
    }
}