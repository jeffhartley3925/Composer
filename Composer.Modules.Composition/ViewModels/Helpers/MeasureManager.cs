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
using System.Collections.ObjectModel;
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

        public static Repository.DataService.Measure Clone(Guid pId, Repository.DataService.Measure m, Collaborator col)
        {
            Repository.DataService.Measure o;
            o = Create(pId, m.Sequence);
            o.Id = Guid.NewGuid();
            o.Staff_Id = pId;
            o.Sequence = m.Sequence;
            o.Index = -1;
            o.Width = (int.Parse(m.Width) + 7).ToString(CultureInfo.InvariantCulture);
            o.Duration = m.Duration;
            o.Bar_Id = m.Bar_Id;
            o.Spacing = m.Spacing;
            o.TimeSignature_Id = m.TimeSignature_Id;
            o.Instrument_Id = m.Instrument_Id;
            o.Status = CollaborationManager.GetBaseStatus();
            foreach (Chord chord in m.Chords)
            {
                o.Chords.Add(ChordManager.Clone(o, chord, col));
            }
            Cache.AddMeasure(o);
            return o;
        }

        public static Repository.DataService.Measure Clone(Guid pId, Repository.DataService.Measure m)
        {
            Repository.DataService.Measure o = null;
            try
            {
                o = Create(pId, m.Sequence);
                o.Id = Guid.NewGuid();
                o.Staff_Id = pId;
                o.Sequence = m.Sequence;
                o.Index = (short)Cache.Measures.Count();
                o.Width = m.Width;
                o.Duration = m.Duration;
                o.Bar_Id = m.Bar_Id;
                o.Spacing = m.Spacing;
                o.TimeSignature_Id = m.TimeSignature_Id;
                o.Instrument_Id = m.Instrument_Id;
                o.Status = CollaborationManager.GetBaseStatus();
                foreach (var ch in m.Chords)
                {
                    o.Chords.Add(ChordManager.Clone(o, ch));
                }
                Cache.AddMeasure(o);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
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
            _ea.GetEvent<ArrangeMeasure>().Subscribe(OnArrangeMeasure);
        }

        public static bool IsPackedMeasure(Repository.DataService.Measure m)
        {
            if (m != null) // m could be null when the very first n of a m is laid
            //down, so when m is null, we know the measeure is not packed.
            {
                if (m.Chords.Any())
                {
                    var chords = ChordManager.GetActiveChords(m);
                    if (chords.Count > 0)
                    {
                        var d = Convert.ToDouble((from c in chords select c.Duration).Sum());
                        return d >= DurationManager.BPM;
                    }
                }
            }
            return false;
        }

        //public static bool IsPackedGroupedMeasure(Repository.DataService._measure m)
        //{
        //    bool bpacked = false;
        //    if (m != null)
        //    {
        //        if (m.Chords.Any())
        //        {
        //            var chs = ChordManager.GetActiveChords(m);
        //            if (chs.Count > 0)
        //            {
        //                var d = Convert.ToDouble((from ach in chs select ach.Duration).Sum());
        //                bpacked = d >= DurationManager.BPM;
        //            }
        //        }
        //        if (!bpacked)  //if bpacked is true then no need to check further - we know the m is packed.
        //        {
        //            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
        //            {
        //                var m_f = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
        //                var m_dens = Composer.Infrastructure.Support.Densities.MeasureDensity;
        //                int m_idx = (m_f.Index == 0) ? m.Index + m_dens : m.Index - m_dens;
        //                m = (from a in Cache.Measures where a.Index == m_idx select a).First();
        //                if (m.Chords.Any())
        //                {
        //                    var chs = ChordManager.GetActiveChords(m);
        //                    if (chs.Count > 0)
        //                    {
        //                        var d = Convert.ToDouble((from ach in chs select ach.Duration).Sum());
        //                        bpacked = d >= DurationManager.BPM;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return bpacked;
        //}

        //public static bool IsPackedForCollaborator(Repository.DataService._measure m)
        //{
        //    bool bpacked = false;
        //    if (m != null)
        //    {
        //        if (m.Chords.Any())
        //        {
        //            var chs = ChordManager.GetActiveChordsForSelectedCollaborator(m);
        //            if (chs.Count > 0)
        //            {
        //                var d = Convert.ToDouble((from ach in chs select ach.Duration).Sum());
        //                bpacked = d >= DurationManager.BPM;
        //            }
        //        }
        //        if (!bpacked)  //if bpacked is true then no need to check further - we know the m is packed.
        //        {
        //            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
        //            {
        //                var m_f = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
        //                var m_dens = Composer.Infrastructure.Support.Densities.MeasureDensity;
        //                int m_idx = (m_f.Index == 0) ? m.Index + m_dens : m.Index - m_dens;
        //                m = (from a in Cache.Measures where a.Index == m_idx select a).First();
        //                if (m.Chords.Any())
        //                {
        //                    var chs = ChordManager.GetActiveChordsForSelectedCollaborator(m);
        //                    if (chs.Count > 0)
        //                    {
        //                        var d = Convert.ToDouble((from ach in chs select ach.Duration).Sum());
        //                        bpacked = d >= DurationManager.BPM;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return bpacked;
        //}

        private static void SetNotegroupContext()
        {
            NotegroupManager.Measure = _measure;
            NotegroupManager.ChordStarttimes = _chordStartTimes;
            NotegroupManager.ChordNotegroups = _chordNotegroups;
            NotegroupManager.Measure = _measure;
            NotegroupManager.Chord = _chord;
        }

        public static void OnArrangeMeasure(Repository.DataService.Measure m)
        {
            //this method calculates m.Spacing then raises the Measure_Loaded event. the m.Spacing property is
            //only used to calculate ch spc when spacingMode is 'constant.' For now, however, we call this method
            //no matter what the spaingMode is becase this method raises the arrangeVerse event and the arrangeVerse event
            //should be raised for all spacingModes. TODO: decouple m spc from verse spc. or at the very least 
            //encapsulate the switch block in 'if then else' block so it only executes when the spacingMode is 'constant'.

            //'EditorState.Ratio * .9' expression needs to be revisited.

            _measure = m;
            ObservableCollection<Chord> chords = ChordManager.GetActiveChords(_measure.Chords);

            if (chords.Count > 0)
            {
                ChordManager.Initialize();
                SetNotegroupContext();
                _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);

                switch (Preferences.MeasureArrangeMode)
                {
                    case _Enum.MeasureArrangeMode.DecreaseMeasureWidth:
                        _ea.GetEvent<AdjustMeasureWidth>().Publish(new Tuple<Guid, double>(_measure.Id, Preferences.MeasureMaximumEditingSpace));
                        break;
                    case _Enum.MeasureArrangeMode.IncreaseMeasureSpacing:
                        m.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(_measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / chords.Count));
                        _ea.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                        break;
                    case _Enum.MeasureArrangeMode.ManualResizePacked:
                        m.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(_measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / _measure.Chords.Count));
                        _ea.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                        break;
                    case _Enum.MeasureArrangeMode.ManualResizeNotPacked:
                        m.Spacing = (int)Math.Ceiling(m.Spacing * EditorState.Ratio * .9);
                        _ea.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                        break;
                }
                if (!EditorState.IsOpening)
                {
                    _ea.GetEvent<ArrangeVerse>().Publish(_measure);
                }
            }
        }

        public static void Flag()
        {
            _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);
            foreach (Decimal st in _chordStartTimes)
            {
                if (_measureChordNotegroups.ContainsKey(st))
                {
                    List<Notegroup> ngs = _measureChordNotegroups[st];
                    foreach (Notegroup ng in ngs)
                    {
                        if (NotegroupManager.HasFlag(ng) && !NotegroupManager.IsRest(ng))
                        {
                            var root = ng.Root;
                            root.Vector_Id = (short)DurationManager.GetVectorId((double)root.Duration);
                        }
                    }
                }
            }
        }
    }
}
