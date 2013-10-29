using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Repository.DataService;
using Measure = Composer.Infrastructure.Constants.Measure;

namespace Composer.Modules.Composition.ViewModels
{
    // TODO: remove _chord.Location_Y from schema
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel
    {
        private int _adjustedLocationX;
        private Chord _chord;

        public ChordViewModel(string id)
        {
            _chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(id));
            if (!EditorState.IsComposing)
            {
                EA.GetEvent<EditorStateChanged>().Publish(true);
            }
            DefineCommands();
            SubscribeEvents();

            EA.GetEvent<NotifyChord>().Publish(Chord.Measure_Id);
            EA.GetEvent<AdjustMeasureEndSpace>().Publish(string.Empty);
        }

        public Chord Chord
        {
            get { return _chord; }
            set
            {
                _chord = value;
                AdjustedLocation_X = _chord.Location_X;
                OnPropertyChanged(() => Chord);
            }
        }

        public int AdjustedLocation_X
        {
            get { return _adjustedLocationX; }
            set
            {
                if (value != _adjustedLocationX)
                {
                    _adjustedLocationX = value;
                    Chord.Location_X = _adjustedLocationX;
                    EA.GetEvent<SynchronizeChord>().Publish(Chord);
                    OnPropertyChanged(() => AdjustedLocation_X);
                }
            }
        }

        #region IChordViewModel Members

        public void DefineCommands()
        {
            ClickCommand = new DelegatedCommand<object>(OnClick);
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<DeleteChord>().Subscribe(OnDelete);
            EA.GetEvent<ChordClicked>().Subscribe(OnChildClick, true);
            EA.GetEvent<SetChordLocationX>().Subscribe(OnSetChordLocationX);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
        }

        #endregion

        private void SetChordContext()
        {
            ChordManager.Vm = this;
            ChordManager.Chord = Chord;
        }

        public void OnDelete(Chord chord)
        {
            if (chord.Id != Chord.Id) return;
            SetChordContext();
            ChordManager.Delete(chord);
        }

        public override void OnClick(object obj)
        {
            var note = (Note) obj;
            SetChordContext();
            ChordManager.Select(note);
        }

        public void OnUpdateChord(Chord chord)
        {
            if (chord.Id != Chord.Id) return;
            Chord = chord;
        }

        public override void OnChildClick(object obj)
        {
            var note = (Note) obj;
            if (Chord.StartTime.ToString() == note.StartTime.ToString())
            {
                OnClick(note);
            }
        }

        public void OnSetChordLocationX(Tuple<Guid, Guid, double> payload)
        {
            // when not collaborating, the location_x in the database, IS the Location_X of the chord.
            // when collaborating, the location_x is variable and depends on whether a collaborator is selected,
            // and what contributions have been accepted and/or rejected by the current user. we call this THE the AdjustedLocation_X

            var chId1 = payload.Item2;
            var chId2 = payload.Item1;
            if (chId2 != Chord.Id) return;

            var mWidthRatio = payload.Item3;

            var ch2 = Utils.GetChord(chId2);
            if (chId1 != Guid.Empty)
            {
                var ch1 = Utils.GetChord(chId1);
                SetAdjustedLocationX(ch1, ch2, mWidthRatio);
                ch2.StartTime = GetChordStarttime(ch1, ch2);
            }
            else
            {
                ch2.StartTime = GetChordStarttime(ch2);
                AdjustedLocation_X = Measure.Padding;
            }
            EA.GetEvent<SynchronizeChord>().Publish(ch2);
            EA.GetEvent<UpdateChord>().Publish(ch2);
        }

        private static double? GetChordStarttime(Chord ch1, Chord ch2)
        {
            return ch1.StartTime + (double)ch2.Duration;
        }

        private static int GetChordStarttime(Chord ch2)
        {
            var m = Utils.GetMeasure(ch2.Measure_Id);
            var mStaff = Utils.GetStaff(m.Staff_Id);
            var mStaffgroup = Utils.GetStaffgroup(mStaff.Staffgroup_Id);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mStarttime = ((m.Index%mDensity)*DurationManager.Bpm) + (mStaffgroup.Index*mDensity*DurationManager.Bpm);
            return mStarttime;
        }

        private void SetAdjustedLocationX(Chord ch1, Chord ch2, double mWidthRatio)
        {
            double spacing = DurationManager.GetProportionalSpace((double)ch2.Duration);
            spacing = spacing * mWidthRatio * EditorState.NoteSpacingRatio;
            AdjustedLocation_X = (int)(Math.Ceiling(ch1.Location_X + spacing));
        }

        public void OnDeSelectComposition(object obj)
        {
            if (IsSelected)
            {
                EA.GetEvent<DeSelectChord>().Publish(Chord.Id);
            }
        }

        public void OnSelectChord(Guid id)
        {
            if (Chord.Id != id) return;
            foreach (var n in Chord.Notes)
            {
                if (CollaborationManager.IsActive(n))
                {
                    EA.GetEvent<SelectNote>().Publish(n.Id);
                }
            }
            ShowSelector();
        }

        public void OnDeSelectChord(Guid id)
        {
            if (Chord.Id != id) return;
            if (!IsSelected) return;

            foreach (var r in Chord.Notes)
            {
                EA.GetEvent<DeSelectNote>().Publish(r.Id);
            }
            HideSelector();
        }
    }
}