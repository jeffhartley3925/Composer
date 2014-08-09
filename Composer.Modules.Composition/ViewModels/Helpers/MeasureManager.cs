using System;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository;

using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	using Composer.Infrastructure.Events;
	using Composer.Repository.DataService;

	public static class MeasureManager
    {
        private static DataServiceRepository<Composition> _repository;

		private static readonly IEventAggregator Ea;

        public static int CurrentDensity { get; set; }

        static MeasureManager()
        {
			Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            CurrentDensity = Defaults.DefaultMeasureDensity;
	        SubscribeEvents();
        }

        public static bool IsEmpty(Measure mE)
        {
            return mE.Chords.Count == 0;
        }

		private static IEnumerable<Chord> _activeChords;
		public static IEnumerable<Chord> ActiveChs
		{
			get { return _activeChords ?? (_activeChords = new List<Chord>()); }
			set
			{
				_activeChords = value;
				_activeChords = new List<Chord>(_activeChords.OrderBy(p => p.StartTime));
			}
		}

        public static Measure Create(Guid sFiD, int sQ)
        {
            var o = _repository.Create<Measure>();
            o.Id = Guid.NewGuid();
            o.Staff_Id = sFiD;
            o.Sequence = sQ;
            o.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            o.Bar_Id = Infrastructure.Dimensions.Bars.Bar.Id;
            o.Instrument_Id = Infrastructure.Dimensions.Instruments.Instrument.Id;
            o.Width = Preferences.MeasureWidth.ToString(CultureInfo.InvariantCulture);
            o.TimeSignature_Id = Infrastructure.Dimensions.TimeSignatures.TimeSignature.Id;
            o.Spacing = EditorState.ChordSpacing;
            o.LedgerColor = Preferences.NoteForeground;
            o.Audit = Common.GetAudit();
            o.Status = CollaborationManager.GetBaseStatus();
            Cache.AddMeasure(o);
            return o;
        }

        public static void Initialize()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Composition>>();
            ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
			Ea.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
        }

		public static  void OnNotifyActiveChords(Tuple<Guid, object, object, object, int, Guid> payload)
		{
			ActiveChs = (ObservableCollection<Chord>)payload.Item4;
		}

		public static double GetMeasureDuration(Measure mE, Collaborator cLr)
		{
			//TODO CollaborationManager.IsActive(cH, cLr) may be redundant with ActiveChs
			return Convert.ToDouble((from cH in ActiveChs where CollaborationManager.IsActive(cH, cLr) select cH.Duration).Sum());
		}

		public static bool GetPackState(Measure mE, Collaborator cLr)
		{
			var dU = GetMeasureDuration(mE, cLr);
			return dU >= DurationManager.Bpm;
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
			        .Select(b => b.IsPackedMeasure));
			return a.Any() ? a.First() : false;
        }
    }
}
