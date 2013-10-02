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

        public static List<Guid> PackedStaffMeasures = null;
        public static List<Tuple<Guid, int>> PackedStaffgroupMeasures = null;

        public static int CurrentDensity { get; set; }

        static MeasureManager()
        {
            CurrentDensity = Defaults.DefaultMeasureDensity;

            if (PackedStaffMeasures == null)
                PackedStaffMeasures = new List<Guid>();

            if (PackedStaffgroupMeasures == null)
                PackedStaffgroupMeasures = new List<Tuple<Guid, int>>();
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
            _ea.GetEvent<UpdatePackedMeasures>().Subscribe(OnUpdatePackedStatus);
        }

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
            // this method calculates measure spacing then raises the Measure_Loaded event. the m.Spacing property is
            // only used to calculate chord spacing when spacingMode is 'constant.' For now, however, we call this method
            // no matter what the spaingMode is because this method raises the arrangeVerse event and the arrangeVerse event
            // should be raised for all spacingModes. TODO: decouple m spacing from verse spacing. or at the very least 
            // encapsulate the switch block in 'if then else' block so it only executes when the spacingMode is 'constant'.

            // 'EditorState.Ratio * .9' expression needs to be revisited.

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
                        m.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(_measure.Width) - Infrastructure.Constants.Measure.Padding * 2) / chords.Count));
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

        #region Packed Measure Functionality

        public static bool IsPackedStaffMeasure(Repository.DataService.Measure m)
        {
            return IsPackedStaffMeasure(m, null);
        }

        public static bool IsPackedStaffMeasure(Repository.DataService.Measure m, Collaborator col)
        {
            if (m == null) return false;
            if (!m.Chords.Any()) return false;
            var chs = ChordManager.GetActiveChords(m, col);
            if (chs.Count <= 0) return false;
            var mDuration = Convert.ToDouble((from c in chs select c.Duration).Sum());
            return mDuration >= DurationManager.BPM;
        }

        public static bool IsPackedStaffgroupMeasure(Repository.DataService.Measure m, Collaborator col)
        {
            if (m == null) return false;

            var packed = IsPackedStaffMeasure(m, col);
            if (packed) return true;

            if (EditorState.StaffConfiguration != _Enum.StaffConfiguration.Grand &&
                EditorState.StaffConfiguration != _Enum.StaffConfiguration.MultiInstrument) return false;

            var mStaff = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).First();
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mIndex = (mStaff.Index == 0) ? m.Index + mDensity : m.Index - mDensity;
            m = (from a in Cache.Measures where a.Index == mIndex select a).First();
            return IsPackedStaffMeasure(m, col);
        }

        public static void OnUpdatePackedStatus(object obj)
        {
            var payload = (Tuple<Repository.DataService.Measure, object>)obj;
            var m = payload.Item1;
            var col = (payload.Item2 == null) ? null : (Collaborator)payload.Item2;
            var mStaff = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).Single();
            var mStaffgroup = (from a in Cache.Staffgroups where a.Id == mStaff.Staffgroup_Id select a).Single();
            var mPackedKey = new Tuple<Guid, int>(mStaffgroup.Id, m.Sequence);

            var isPacked = IsPackedStaffMeasure(m, col);
            if (isPacked)
            {
                if (!PackedStaffMeasures.Contains(m.Id)) PackedStaffMeasures.Add(m.Id);
                if (!PackedStaffgroupMeasures.Contains(mPackedKey)) PackedStaffgroupMeasures.Add(mPackedKey);
            }
            else
            {
                if (PackedStaffMeasures.Contains(m.Id)) PackedStaffMeasures.Remove(m.Id);
            }
            if (isPacked) return;
            isPacked = IsPackedStaffgroupMeasure(m, col);
            if (isPacked)
            {
                if (!PackedStaffgroupMeasures.Contains(mPackedKey)) PackedStaffgroupMeasures.Add(mPackedKey);
            }
            else
            {
                if (PackedStaffgroupMeasures.Contains(mPackedKey)) PackedStaffgroupMeasures.Remove(mPackedKey);
            }
        }

        public static bool IsPacked(Repository.DataService.Measure m)
        {
            return IsPacked(m, _Enum.PackedMeasureScope.Staff);
        }

        public static bool IsPacked(Repository.DataService.Measure m, _Enum.PackedMeasureScope scope)
        {
            var result = false;
            switch (scope)
            {
                case _Enum.PackedMeasureScope.Staff:
                    result = PackedStaffMeasures.Contains(m.Id);
                    break;
                case _Enum.PackedMeasureScope.Staffgroup:
                    var mStaff = (from a in Cache.Staffs where a.Id == m.Staff_Id select a).Single();
                    var mStaffgroup = (from a in Cache.Staffgroups where a.Id == mStaff.Staffgroup_Id select a).Single();
                    var mPackedKey = new Tuple<Guid, int>(mStaffgroup.Id, m.Sequence);
                    result = PackedStaffgroupMeasures.Contains(mPackedKey);
                    break;
            }
            return result;
        }

        #endregion
    }
}
