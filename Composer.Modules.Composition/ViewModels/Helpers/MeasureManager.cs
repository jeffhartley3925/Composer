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
        private static Repository.DataService.Measure Measure { get; set; }
        private static Chord Chord { get; set; }
        private static Dictionary<decimal, List<Notegroup>> _measureChordNotegroups;
        private static List<Notegroup> ChordNotegroups { get; set; }
        private static decimal[] _chordStartTimes;
        private static decimal[] _chordInactiveTimes;

        public static int CurrentDensity = Defaults.DefaultMeasureDensity;

        static MeasureManager()
        {
        }

        public static bool IsEmpty(Repository.DataService.Measure measure)
        {
            return measure.Chords.Count == 0;
        }

        public static Repository.DataService.Measure Create(Guid parentId, int sequence)
        {
            var obj = _repository.Create<Repository.DataService.Measure>();
            obj.Id = Guid.NewGuid();
            obj.Staff_Id = parentId;
            obj.Sequence = sequence;
            obj.Key_Id = Infrastructure.Dimensions.Keys.Key.Id;
            obj.Bar_Id = Infrastructure.Dimensions.Bars.Bar.Id;
            obj.Instrument_Id = Infrastructure.Dimensions.Instruments.Instrument.Id;
            obj.Width = Preferences.MeasureWidth.ToString(CultureInfo.InvariantCulture);
            obj.TimeSignature_Id = Infrastructure.Dimensions.TimeSignatures.TimeSignature.Id;
            obj.Spacing = EditorState.ChordSpacing;
            obj.LedgerColor = Preferences.NoteForeground;
            obj.Audit = Common.GetAudit();
            obj.Status = CollaborationManager.GetBaseStatus();
            Cache.AddMeasure(obj);
            return obj;
        }

        public static Repository.DataService.Measure Clone(Guid parentId, Repository.DataService.Measure source, Collaborator collaborator)
        {
            Repository.DataService.Measure obj;
            obj = Create(parentId, source.Sequence);
            obj.Id = Guid.NewGuid();
            obj.Staff_Id = parentId;
            obj.Sequence = source.Sequence;
            obj.Index = -1;
            obj.Width = (int.Parse(source.Width) + 7).ToString(CultureInfo.InvariantCulture);
            obj.Duration = source.Duration;
            obj.Bar_Id = source.Bar_Id;
            obj.Spacing = source.Spacing;
            obj.TimeSignature_Id = source.TimeSignature_Id;
            obj.Instrument_Id = source.Instrument_Id;
            obj.Status = CollaborationManager.GetBaseStatus();
            foreach (Chord chord in source.Chords)
            {
                obj.Chords.Add(ChordManager.Clone(obj, chord, collaborator));
            }
            Cache.AddMeasure(obj);
            return obj;
        }

        public static Repository.DataService.Measure Clone(Guid parentId, Repository.DataService.Measure source)
        {
            Repository.DataService.Measure obj = null;
            try
            {
                obj = Create(parentId, source.Sequence);
                obj.Id = Guid.NewGuid();
                obj.Staff_Id = parentId;
                obj.Sequence = source.Sequence;
                obj.Index = (short)Cache.Measures.Count();
                obj.Width = source.Width;
                obj.Duration = source.Duration;
                obj.Bar_Id = source.Bar_Id;
                obj.Spacing = source.Spacing;
                obj.TimeSignature_Id = source.TimeSignature_Id;
                obj.Instrument_Id = source.Instrument_Id;
                obj.Status = CollaborationManager.GetBaseStatus();
                foreach (var chord in source.Chords)
                {
                    obj.Chords.Add(ChordManager.Clone(obj, chord));
                }
                Cache.AddMeasure(obj);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return obj;
        }

        public static void Initialize()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();

            Chord = null;
            ChordNotegroups = null;

            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
            _ea.GetEvent<ArrangeMeasure>().Subscribe(OnArrangeMeasure);
        }

        public static bool IsPackedMeasure(Repository.DataService.Measure m)
        {
            if (m != null) // m could be null when the very first n of a measure is laid
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

        //public static bool IsPackedGroupedMeasure(Repository.DataService.Measure m)
        //{
        //    bool bpacked = false;
        //    if (m != null)
        //    {
        //        if (m.Chords.Any())
        //        {
        //            var chords = ChordManager.GetActiveChords(m);
        //            if (chords.Count > 0)
        //            {
        //                var d = Convert.ToDouble((from c in chords select c.Duration).Sum());
        //                bpacked = d >= DurationManager.BPM;
        //            }
        //        }
        //        if (!bpacked)  //if bpacked is true then no need to check further - we know the measure is packed.
        //        {
        //            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
        //            {
        //                var m_f = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
        //                var m_dens = Composer.Infrastructure.Support.Densities.MeasureDensity;
        //                int m_idx = (m_f.Index == 0) ? m.Index + m_dens : m.Index - m_dens;
        //                m = (from a in Cache.Measures where a.Index == m_idx select a).First();
        //                if (m.Chords.Any())
        //                {
        //                    var chords = ChordManager.GetActiveChords(m);
        //                    if (chords.Count > 0)
        //                    {
        //                        var d = Convert.ToDouble((from c in chords select c.Duration).Sum());
        //                        bpacked = d >= DurationManager.BPM;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return bpacked;
        //}

        //public static bool IsPackedForCollaborator(Repository.DataService.Measure m)
        //{
        //    bool bpacked = false;
        //    if (m != null)
        //    {
        //        if (m.Chords.Any())
        //        {
        //            var chords = ChordManager.GetActiveChordsForSelectedCollaborator(m);
        //            if (chords.Count > 0)
        //            {
        //                var d = Convert.ToDouble((from c in chords select c.Duration).Sum());
        //                bpacked = d >= DurationManager.BPM;
        //            }
        //        }
        //        if (!bpacked)  //if bpacked is true then no need to check further - we know the measure is packed.
        //        {
        //            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand)
        //            {
        //                var m_f = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
        //                var m_dens = Composer.Infrastructure.Support.Densities.MeasureDensity;
        //                int m_idx = (m_f.Index == 0) ? m.Index + m_dens : m.Index - m_dens;
        //                m = (from a in Cache.Measures where a.Index == m_idx select a).First();
        //                if (m.Chords.Any())
        //                {
        //                    var chords = ChordManager.GetActiveChordsForSelectedCollaborator(m);
        //                    if (chords.Count > 0)
        //                    {
        //                        var d = Convert.ToDouble((from c in chords select c.Duration).Sum());
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
            NotegroupManager.Measure = Measure;
            NotegroupManager.ChordStarttimes = _chordStartTimes;
            NotegroupManager.ChordNotegroups = ChordNotegroups;
            NotegroupManager.Measure = Measure;
            NotegroupManager.Chord = Chord;
        }

        public static void OnArrangeMeasure(Repository.DataService.Measure measure)
        {
            //this method calculates measure.Spacing then raises the Measure_Loaded event. the measure.Spacing property is
            //only used to calculate chord spacing when spacingMode is 'constant.' For now, however, we call this method
            //no matter what the spaingMode is becase this method raises the arrangeVerse event and the arrangeVerse event
            //should be raised for all spacingModes. TODO: decouple measure spacing from verse spacing. or at the very least 
            //encapsulate the switch block in 'if then else' block so it only executes when the spacingMode is 'constant'.

            //'EditorState.Ratio * .9' expression needs to be revisited.

            Measure = measure;
            ObservableCollection<Chord> chords = ChordManager.GetActiveChords(Measure.Chords);

            if (chords.Count > 0)
            {
                ChordManager.Initialize();
                SetNotegroupContext();
                _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);

                switch (Preferences.MeasureArrangeMode)
                {
                    case _Enum.MeasureArrangeMode.DecreaseMeasureWidth:
                        _ea.GetEvent<AdjustMeasureWidth>().Publish(new Tuple<Guid, double>(Measure.Id, Preferences.MeasureMaximumEditingSpace));
                        break;
                    case _Enum.MeasureArrangeMode.IncreaseMeasureSpacing:
                        measure.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(Measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / chords.Count));
                        _ea.GetEvent<MeasureLoaded>().Publish(Measure.Id);
                        break;
                    case _Enum.MeasureArrangeMode.ManualResizePacked:
                        measure.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(Measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / Measure.Chords.Count));
                        _ea.GetEvent<MeasureLoaded>().Publish(Measure.Id);
                        break;
                    case _Enum.MeasureArrangeMode.ManualResizeNotPacked:
                        measure.Spacing = (int)Math.Ceiling(measure.Spacing * EditorState.Ratio * .9);
                        _ea.GetEvent<MeasureLoaded>().Publish(Measure.Id);
                        break;
                }
                if (!EditorState.IsOpening)
                {
                    _ea.GetEvent<ArrangeVerse>().Publish(Measure);
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
                            var r = ng.Root;
                            r.Vector_Id = (short)DurationManager.GetVectorId((double)r.Duration);
                        }
                    }
                }
            }
        }
    }
}
