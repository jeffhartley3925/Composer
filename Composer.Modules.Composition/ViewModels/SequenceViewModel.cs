using System.Collections.ObjectModel;
using System.Linq;

namespace Composer.Modules.Composition.ViewModels
{
	using System;
	using System.Collections.Generic;

	using Composer.Infrastructure;
	using Composer.Infrastructure.Events;
	using Composer.Modules.Composition.Annotations;
	using Composer.Modules.Composition.ViewModels.Helpers;
	using Composer.Repository.DataService;

	using Microsoft.Practices.Composite.Events;
	using Microsoft.Practices.ServiceLocation;

    public class SequenceViewModel : BaseViewModel, ISequenceViewModel, IEventCatcher
    {
        private IEventAggregator ea;

        public int SequenceIndex { get; set; }

	    public SequenceViewModel(string sequence)
	    {
	        this.SequenceIndex = int.Parse(sequence);
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
	    }

        public Chord LastChord { get; set; }

        private IEnumerable<Chord> _activeChords;
        public IEnumerable<Chord> ActiveChords
        {
            get { return _activeChords ?? (_activeChords = new List<Chord>()); }
            set
            {
                _activeChords = value;
                _activeChords = new List<Chord>(_activeChords.OrderBy(p => p.StartTime));
            }
        }

        public void OnResizeSequence(object obj)
        {
            var payload = (WidthChange) obj;
            if (payload.Sequence == this.SequenceIndex)
            {
                ea.GetEvent<ResizeMeasure>().Publish((WidthChange)obj);
            }
        }

		public void OnRespaceSequence(Tuple<Guid, int?> payload)
		{
			if (payload.Item2 == null) return;
			var sQIdx = (int)payload.Item2;
			if (!this.IsTargetVM(sQIdx)) return;
			var sQ = SequenceManager.GetSequence(sQIdx);
			foreach (var mG in sQ.Measuregroups)
			{
				ea.GetEvent<RespaceMeasuregroup>().Publish(mG.Id);
			}
		}

		public void OnNotifyActiveChords(Tuple<Guid, object, object, object, int, Guid> payload)
        {
            var sQiDx = payload.Item5;
            if (!IsTargetVM(sQiDx)) return;
            ActiveChords = (ObservableCollection<Chord>)payload.Item3;
            LastChord = (from c in ActiveChords select c).Last();
        }

        public void SubscribeEvents()
        {
            ea.GetEvent<ResizeSequence>().Subscribe(OnResizeSequence);
			ea.GetEvent<RespaceSequence>().Subscribe(OnRespaceSequence);
			ea.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
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