using System;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Constants;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;
using Composer.Repository;

using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public static class MeasureManager
    {
        private static IEventAggregator _ea;
        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        private static Repository.DataService.Measure _measure;
        private static Chord _chord;

        private static Dictionary<decimal, List<Notegroup>> _measureChordNotegroups;
        private static List<Notegroup> _chordNotegroups;
        private static decimal[] _chordStartTimes;
        private static decimal[] _chordInactiveTimes;

        public static int CurrentDensity { get; set; }

        static MeasureManager()
        {
            CurrentDensity = Defaults.DefaultMeasureDensity;
        }

        public static bool IsEmpty(Repository.DataService.Measure m)
        {
            return m.Chords.Count == 0;
        }

        public static Repository.DataService.Measure Create(Guid pId, int seq)
        {
            var o = _repository.Create<Repository.DataService.Measure>();
            o.Id = Guid.NewGuid();
            o.Staff_Id = pId;
            o.Sequence = seq;
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
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();

            _chord = null;
            _chordNotegroups = null;

            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
        }

        private static void SetNotegroupContext()
        {
            NotegroupManager.Measure = _measure;
            NotegroupManager.ChordStarttimes = _chordStartTimes;
            NotegroupManager.ChordNotegroups = _chordNotegroups;
            NotegroupManager.Measure = _measure;
            NotegroupManager.Chord = _chord;
        }

        public static void Flag()
        {
            _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);
            foreach (var st in _chordStartTimes)
            {
                if (!_measureChordNotegroups.ContainsKey(st)) continue;
                var ngs = _measureChordNotegroups[st];
                foreach (Notegroup ng in ngs)
                {
                    if (!NotegroupManager.HasFlag(ng) || NotegroupManager.IsRest(ng)) continue;
                    var root = ng.Root;
                    root.Vector_Id = (short)DurationManager.GetVectorId((double)root.Duration);
                }
            }
        }

        public static bool IsPacked(Repository.DataService.Measure m)
        {
            return (Statistics.MeasureStatistics.Where(
                b => b.MeasureId == m.Id && b.CollaboratorIndex == 0).Select(b => b.IsPacked)).First();
        }
    }
}
