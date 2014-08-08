using System.Collections.ObjectModel;
using System.Linq;

using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels
{
	using System;
	using System.Collections.Generic;

	using Composer.Infrastructure;

    public class MeasuregroupViewModel : BaseViewModel, IEventCatcher
    {
        private IEventAggregator ea;

	    private double spacingRatio = 1;

		private double threshholdStarttime = 0;

        private Guid Id;

	    public MeasuregroupViewModel(string mGiD)
	    {
	        if (string.IsNullOrWhiteSpace(mGiD)) return;
	        Id = Guid.Parse(mGiD);
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
	        SubscribeEvents();
	    }

        public Chord LastChord { get; set; }

        private IEnumerable<Chord> _activeChords;
        public IEnumerable<Chord> ActiveChs
        {
            get { return _activeChords ?? (_activeChords = new List<Chord>()); }
            set
            {
                _activeChords = value;
                _activeChords = new List<Chord>(_activeChords.OrderBy(p => p.StartTime));
            }
        }

        public void OnNotifyActiveChords(Tuple<Guid, object, object, object, int, Guid> payload)
        {
            var mGiD = payload.Item6;
            if (!IsTargetVM(mGiD)) return;
            this.ActiveChs = (ObservableCollection<Chord>)payload.Item4;
			LastChord = (ActiveChs.Any()) ? (from c in this.ActiveChs select c).Last() : null;
        }

        public void OnRespaceMeasuregroup(Guid mGiD)
        {
            if (IsTargetVM(mGiD) && !EditorState.IsOpening)
            {
                Chord prevCh = null;
                var distinctStChs = this.ActiveChs.GroupBy(p => p.StartTime).Select(g => g.First()).ToList();
                foreach (var cH in distinctStChs)
                {
					if (cH.StartTime < threshholdStarttime)
					{
						prevCh = cH;
						continue;
					}
                    var x = (prevCh == null) ? 7 : ChordManager.GetProportionalLocationX(prevCh, spacingRatio);
                    if (cH.StartTime != null)
                        ea.GetEvent<SetChordLocationX>().Publish(new Tuple<Guid, int, double>(cH.Id, x, (double)cH.StartTime));
                    prevCh = cH;
                }
				ArrangeMeasure(mGiD);
				threshholdStarttime = 0;
            }
        }

		/// <summary>
		/// Respace lyrics. redraw slurs and ties, etc
		/// </summary>
		/// <param name="mGiD"></param>
        private void ArrangeMeasure(Guid mGiD)
        {
            var mG = MeasuregroupManager.GetMeasuregroup(mGiD);
            foreach (var mE in mG.Measures)
            {
                ea.GetEvent<ArrangeMeasure>().Publish(mE.Id);
            }
        }

		public void OnSetThreshholdStarttime(Tuple<Guid, double> payload)
		{
			var mGiD = payload.Item1;
			if (!IsTargetVM(mGiD)) return;
			threshholdStarttime = payload.Item2;
		}
		
        /// <summary>
		/// This event is thrown n times where n is the number of measures in the measure group. We only need to catch this
		/// event, once per resize action since all measures in the measure group use the same spacing ratio. so the measure 
		/// group spacingRatio is purposefully taken from the measure containing the last chord in the measure group. see
        /// IsTargetVM(Guid mGiD, Guid mEiD)
		/// </summary>
		/// <param name="payload">Item1 = Measuregroup.Id, Item2 = Measure.Id, Item3 = ratio</param>
		public void OnUpdateMeasureSpacingRatio(Tuple<Guid, Guid, double> payload)
		{
			Guid mGiD = payload.Item1;
            Guid mEiD = payload.Item2;
            if (IsTargetVM(mGiD, mEiD))
			{
				this.spacingRatio = payload.Item3;
			}
		}

        public void SubscribeEvents()
        {
            ea.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
            ea.GetEvent<RespaceMeasuregroup>().Subscribe(OnRespaceMeasuregroup);
			ea.GetEvent<UpdateMeasureSpacingRatio>().Subscribe(OnUpdateMeasureSpacingRatio);
			ea.GetEvent<SetThreshholdStarttime>().Subscribe(OnSetThreshholdStarttime);
        }

        public void DefineCommands()
        {
            throw new NotImplementedException();
        }

        public bool IsTargetVM(Guid mGiD)
        {
            return Id == mGiD;
        }

        public bool IsTargetVM(Guid mGiD, Guid mEiD)
        {
			if (LastChord == null) return false;
            return this.Id == mGiD && LastChord.Measure_Id == mEiD;
        }
    }
}
