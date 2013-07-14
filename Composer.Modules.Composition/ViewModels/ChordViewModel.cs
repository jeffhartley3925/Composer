using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Repository.DataService;
using Measure = Composer.Infrastructure.Constants.Measure;

namespace Composer.Modules.Composition.ViewModels
{
    //TODO: remove Chord.Location_Y from schema
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel
    {
        private int _adjustedLocationX;
        private Chord _chord;

        public ChordViewModel(string id)
        {
            _chord = null;
            HideSelector();
            Chord = (from obj in Cache.Chords where obj.Id == Guid.Parse(id) select obj).First();
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
            ChordManager.ViewModel = this;
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

        public void OnSetChordLocationX(Tuple<int, int, Guid, Guid, int, double> payload)
        {
            //when not collaborating, the location_x in the database, IS the Location_X of the chord.
            //when collaborating, the location_x is variable and depends on whether a collaborator is selected,
            //and who the collaborator is.
            if (payload.Item3 != Chord.Id) return;

            //load payload into descriptive variables
            var actualChordIndex = payload.Item1;
            var runningInactiveChordCnt = payload.Item2;
            var chordId = payload.Item3;
            var prevChordId = payload.Item4;
            var measureSpacing = payload.Item5;
            var measureWidthChangeRatio = payload.Item6;

            switch (Preferences.SpacingMode)
            {
                case _Enum.MeasureSpacingMode.Constant:
                    AdjustedLocation_X = (actualChordIndex - runningInactiveChordCnt) * measureSpacing;
                    break;
                case _Enum.MeasureSpacingMode.Proportional:
                    var chord = (from a in Cache.Chords where a.Id == chordId select a).First();
                    var parentMeasure = (from a in Cache.Measures where a.Id == chord.Measure_Id select a).First();
                    if (prevChordId != Guid.Empty)
                    {
                        var previousChord = (from a in Cache.Chords where a.Id == prevChordId select a).First();
                        double proportionalSpace = DurationManager.GetProportionalSpace((double) chord.Duration);
                        proportionalSpace = proportionalSpace * measureWidthChangeRatio * EditorState.NoteSpacingRatio;
                        AdjustedLocation_X = (int) (Math.Ceiling(previousChord.Location_X + proportionalSpace));
                    }
                    else
                    {
                        AdjustedLocation_X = Measure.Padding;
                    }
                    break;
            }
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
            foreach (Note note in Chord.Notes)
            {
                if (CollaborationManager.IsActive(note))
                {
                    EA.GetEvent<SelectNote>().Publish(note.Id);
                }
            }
            ShowSelector();
        }

        public void OnDeSelectChord(Guid id)
        {
            if (Chord.Id != id) return;
            if (IsSelected)
            {
                foreach (Note note in Chord.Notes)
                {
                    EA.GetEvent<DeSelectNote>().Publish(note.Id);
                }
                HideSelector();
            }
        }
    }
}