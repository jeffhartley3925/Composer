using System;
using System.Collections.ObjectModel;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.ServiceLocation;
using Measure = Composer.Infrastructure.Constants.Measure;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels
{
    // TODO: remove _chord.Location_Y from schema
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel
    {
        private DataServiceRepository<Repository.DataService.Composition> _repository;

        private int _adjustedLocationX;
        private SpacingHelper _spacer;
        private List<ChordProjection> _chordProjections;
        public Chord LastMeasureGroupChord { get; set; }
        public Chord LastSequenceChord { get; set; }
        private IEnumerable<Chord> _activeMeasureGroupChords;
        public IEnumerable<Chord> ActiveMeasureGroupChords
        {
            get { return _activeMeasureGroupChords ?? (_activeMeasureGroupChords = new ObservableCollection<Chord>()); }
            set
            {
                _activeMeasureGroupChords = value;
                _activeMeasureGroupChords = new ObservableCollection<Chord>(_activeMeasureGroupChords.OrderBy(p => p.StartTime));
            }
        }

        private IEnumerable<Chord> _activeSequenceChords;
        public IEnumerable<Chord> ActiveSequenceChords
        {
            get { return _activeSequenceChords ?? (_activeSequenceChords = new ObservableCollection<Chord>()); }
            set
            {
                _activeSequenceChords = value;
                _activeSequenceChords = new ObservableCollection<Chord>(_activeSequenceChords.OrderBy(p => p.StartTime));
            }
        }

        private ObservableCollection<Chord> _activeChords;
        public ObservableCollection<Chord> ActiveChords
        {
            get { return _activeChords ?? (_activeChords = new ObservableCollection<Chord>()); }
            set
            {
                _activeChords = value;
                _activeChords = new ObservableCollection<Chord>(_activeChords.OrderBy(p => p.StartTime));
            }
        }

        public ChordViewModel(string id)
        {
            Chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(id));
            var m = Utils.GetMeasure(Chord.Measure_Id);
            SetRepository();
            SubscribeEvents();
            DefineCommands();
            if (!EditorState.IsOpening)
            {
                EA.GetEvent<AdjustChords>().Publish(new Tuple<Guid, bool, Guid>(m.Id, false, Chord.Id));
                EA.GetEvent<BumpMeasureWidth>().Publish(new Tuple<Guid, double, int>(Chord.Measure_Id, Preferences.M_END_SPC, m.Sequence));
            }
            else
            {
                if (EditorState.IsOpening)
                {
                    SetActiveChordCount();

                    if (CheckAllActiveChordsLoaded())
                    {
                        EditorState.ComposeReadyState = 1;
                    }
                }
            }
            EA.GetEvent<NotifyChord>().Publish(Chord.Measure_Id);
        }

        private static bool CheckAllActiveChordsLoaded()
        {
            EditorState.LoadedActiveChordCount++;
            return EditorState.ActiveChordCount <= EditorState.LoadedActiveChordCount;
        }

        private static void SetActiveChordCount()
        {
            EditorState.ActiveChordCount = Cache.Chords.Count;
        }

        private Chord _chord;
        public Chord Chord
        {
            get { return _chord; }
            set
            {
                _chord = value;
                if (_chord != null)
                {
                    AdjustedLocationX = _chord.Location_X;
                    OnPropertyChanged(() => Chord);
                }

            }
        }

        public int AdjustedLocationX
        {
            get { return _adjustedLocationX; }
            set
            {
                if (value != _adjustedLocationX)
                {
                    _adjustedLocationX = value;
                    Chord.Location_X = _adjustedLocationX;
                    EA.GetEvent<SynchronizeChord>().Publish(Chord);
                    OnPropertyChanged(() => AdjustedLocationX);
                }
            }
        }

        public void DefineCommands()
        {
            ClickCommand = new DelegatedCommand<object>(OnClick);
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<SetThreshholdStarttime>().Subscribe(OnSetThreshholdStarttime);
            EA.GetEvent<SynchronizeChord>().Subscribe(OnSynchronizeChord);
            EA.GetEvent<DeleteChord>().Subscribe(OnDeleteChord);
            EA.GetEvent<ChordClicked>().Subscribe(OnChildClick, true);
            EA.GetEvent<SetChordLocationAndStarttime>().Subscribe(OnSetChordLocationXAndStarttime);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
            EA.GetEvent<MeasureViewModel.SendChordsProjection>().Subscribe(OnSendChordProjection);
            EA.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
        }

        public void OnNotifyActiveChords(Tuple<Guid, object, object, object> payload)
        {
            var id = payload.Item1;
            if (id != Chord.Measure_Id) return;
            ActiveChords = (ObservableCollection<Chord>)payload.Item2;
            ActiveSequenceChords = (ObservableCollection<Chord>)payload.Item3;
            ActiveMeasureGroupChords = (ObservableCollection<Chord>)payload.Item4;
            LastSequenceChord = (from c in ActiveSequenceChords select c).Last();
            LastMeasureGroupChord = (from c in ActiveMeasureGroupChords select c).Last();
        }

        public void OnSendChordProjection(Tuple<Guid, List<ChordProjection>> payload)
        {
            if (Chord.Measure_Id != payload.Item1) return;
            _chordProjections = payload.Item2;
        }

        public void OnSetThreshholdStarttime(Tuple<Guid, double> payload)
        {

        }

        public void OnSynchronizeChord(Chord ch)
        {
            // when the chord starttime or location of a ch changes, then it's constituent notes must be synchronized with the ch. 
            if (ch.Id != Chord.Id) return;
            var ns = ChordManager.GetActiveNotes(ch.Notes);
            foreach (var n in ns)
            {
                if (n.StartTime == ch.StartTime && n.Location_X == ch.Location_X) continue;
                n.StartTime = ch.StartTime;
                n.Location_X = ch.Location_X;
                EA.GetEvent<UpdateNote>().Publish(n);
                _repository.Update(n);
            }
            EA.GetEvent<UpdateChord>().Publish(ch);
        }

        public void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        public void OnDeleteChord(Chord ch)
        {
            if (Chord == null || ch == null)
            {

            }
            if (ch.Id != Chord.Id) return;
            var m = Utils.GetMeasure(Chord.Measure_Id);
            Note rest;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last note in the chord, and the composition is not under collaboration
                // then delete the chord from the DB and insert a rest in it's place.
                if (Chord.Notes.Count == 0)
                {
                    m.Chords.Remove(ch);
                    Cache.Chords.Remove(ch);
                    _repository.Delete(ch);

                    rest = InsertRest(ch, m);
                    if (rest == null)
                    {
                        //throw error
                        return;

                    }
                }
            }
            else
            {
                // if isCollaboration, and all notes in the chord are inactive, then start the
                // flow that replaces the chord with a rest.
                if (!CollaborationManager.IsActive(ch))
                {
                    rest = InsertRest(ch, m);
                    if (rest == null)
                    {
                        //throw error
                        return;

                    }
                    // the n is already deleted marked as purged. we just need to determine the appropriate status for the n.
                    // if the deleted n was purge-able (see NoteController) then it was deleted from the DB and the n status
                    // is set as if it was a normal add to the maxChordsMeasureInSequence.
                    if (EditorState.Purgable)
                    {
                        if (EditorState.EditContext == (int)_Enum.EditContext.Authoring)
                        {
                            rest.Status = Collaborations.SetStatus(rest, (int)_Enum.Status.AuthorAdded);
                            rest.Status = Collaborations.SetAuthorStatus(rest, (int)_Enum.Status.AuthorOriginal);
                        }
                        else
                        {
                            rest.Status = Collaborations.SetStatus(rest, (int)_Enum.Status.ContributorAdded, Collaborations.Index);
                            rest.Status = Collaborations.SetAuthorStatus(rest, (int)_Enum.Status.PendingAuthorAction);
                        }
                        EditorState.Purgable = false;
                    }
                    else
                    {
                        // if note was not purgeable (see NoteController) it must be retained with it's status marked WaitingOn....
                        // the actual status won't be resolved until the note author chooses to reject or accept the note deletion.

                        // another way to say it: the logged in user deleted this note. it's the last note in the chord so the chord is 
                        // replaced by a rest but we can't delete the note because the other collaborator may not want to accept 
                        // the delete. so there is a rest and a chord occupying the same starttime. if the collaborator accepts 
                        // the delete, the note can be purged and the rest has its status set appropriately. if the delete is 
                        // rejected, both remain at the same starttime and the rest has its status set appropriately (see NoteViewModel.OnRejectChange)

                        rest.Status = (EditorState.EditContext == (int)_Enum.EditContext.Authoring) ?
                            Collaborations.SetStatus(rest, (short)_Enum.Status.WaitingOnContributor, 0) :
                            Collaborations.SetStatus(rest, (short)_Enum.Status.WaitingOnAuthor, Collaborations.Index);
                    }
                    Cache.Notes.Add(rest);
                    ch.Notes.Add(rest);
                    _repository.Update(ch);
                }
            }
            EA.GetEvent<DeleteTrailingRests>().Publish(string.Empty);
            var chords = ChordManager.GetActiveChords(m.Chords);
            if (chords.Count <= 0) return;
            EA.GetEvent<UpdateSpanManager>().Publish(m.Id);
            EA.GetEvent<SpanMeasure>().Publish(m.Id);
        }

        private Note InsertRest(Chord ch, Repository.DataService.Measure m)
        {
            EditorState.Duration = (double)ch.Duration;
            EditorState.SetRestContext();
            if (ch.StartTime != null)
            {
                Chord chord = ChordManager.AddChord(m.Id, (double)ch.StartTime);
                chord.Location_X = ch.Location_X;

                Note rest = NoteController.Create(chord, m);
                rest = NoteController.Activate(rest);
                rest.Pitch = Defaults.RestSymbol;
                rest.Location_X = ch.Location_X;
                rest.Location_Y = Finetune.Measure.RestLocationY;
                Cache.Notes.Add(rest);
                chord.Notes.Add(rest);
                _repository.Update(rest);
                Cache.Chords.Add(chord);
                m.Chords.Add(chord);
                _repository.Update(chord);
                _repository.Update(m);
                EA.GetEvent<SynchronizeChord>().Publish(chord);
                return rest;
            }
            return null;
        }

        public override void OnClick(object obj)
        {
            var note = (Note)obj;
            Select(note);
        }

        public void OnUpdateChord(Chord chord)
        {
            if (chord.Id != Chord.Id) return;
            Chord = chord;
        }

        public override void OnChildClick(object obj)
        {
            var note = (Note)obj;
            if (Chord.StartTime.ToString() == note.StartTime.ToString())
            {
                OnClick(note);
            }
        }

        public void OnSetChordLocationXAndStarttime(Tuple<Guid, Guid, double, bool, bool> payload)
        {
            var activeChId = payload.Item1;
            if (activeChId != Chord.Id) return;
            var isResizeStartM = payload.Item4;
            var dontSynchMgSpacing = payload.Item5;
            var prevChId = payload.Item2;
            var ratio = payload.Item3;
            var activeCh = Utils.GetChord(activeChId);
            var m = Utils.GetMeasure(activeCh.Measure_Id);
            if (prevChId != Guid.Empty)
            {
                var prevCh = Utils.GetChord(prevChId);
                AdjustedLocationX = activeCh.Location_X;
                activeCh.StartTime = GetChordStarttime(prevCh, activeCh);
                _spacer = null;

                var chX = GetLocationX(prevCh, activeCh, AdjustedLocationX, ratio, isResizeStartM, dontSynchMgSpacing);
                if (chX != null)
                {
                    AdjustedLocationX = (int)chX;
                    ShiftChords(activeCh, prevCh, ratio);
                }
                else
                {
                    activeCh.StartTime = GetChordStarttime(activeCh);
                    AdjustedLocationX = Measure.Padding;
                }
            }
        }

        private void ShiftChords(Chord activeCh, Chord prevCh, double ratio)
        {
            if (EditorState.IsOpening) return;
            if (_spacer == null) return;
            if (_spacer.LeftChord == null) return;
            if (_spacer.LeftChord.StartTime == null) return;

            int startX;
            if (_spacer.LeftChord != null)
            {
                if (_spacer.LeftChord.Location_X >= activeCh.Location_X &&
                    activeCh.StartTime > _spacer.LeftChord.StartTime)
                {
                    //back shift
                    startX = (int)Math.Ceiling((activeCh.Location_X - (activeCh.Location_X - prevCh.Location_X) / 2));
                    EA.GetEvent<ShiftChords>().Publish(new Tuple<Guid, int, double, int, Guid>(_spacer.ActiveMeasure.Id, _spacer.Sequence, (double)_spacer.LeftChord.StartTime, startX, activeCh.Id));
                    return;
                }
                //forward shift
                startX = activeCh.Location_X + ChordManager.GetProportionalSpacing(prevCh, ratio);
                EA.GetEvent<ShiftChords>().Publish(new Tuple<Guid, int, double, int, Guid>(_spacer.ActiveMeasure.Id, _spacer.Sequence, (double)_spacer.Starttime, startX, activeCh.Id));
            }
        }

        private Chord GetLowestDurationChord(Chord prevCh, Chord activeCh)
        {
            if (activeCh == null) return prevCh;
            return (prevCh.Duration < activeCh.Duration) ? prevCh : activeCh;
        }

        private int? GetLocationX(Chord prevCh, Chord activeCh, int defaultX, double ratio, bool isResizeStartM, bool dontSynchMgSpacing)
        {
            //return defaultX;
            if (EditorState.IsOpening) return defaultX;
            if (isResizeStartM || dontSynchMgSpacing) return ChordManager.GetProportionalLocationX(prevCh, ratio);

            double? activeChSt = activeCh.StartTime;
            if (activeChSt == null) return defaultX;
            var activeM = Utils.GetMeasure(prevCh.Measure_Id);
            var chX = GetLocXByExistingChSt(activeM, activeChSt);
            if (chX != null) return chX;
            chX = GetLocXFromAdjacentChs(activeM, activeChSt, ratio);
            if (chX != null) return chX;
            chX = GetLocXByLastChInMg(ratio, GetLowestDurationChord(prevCh, LastMeasureGroupChord), prevCh, activeCh);
            return chX;
        }

        private int? GetLocXByLastChInMg(double ratio, Chord maxStChInMg, Chord prevCh, Chord activeCh)
        {
            return (maxStChInMg.Measure_Id == activeCh.Measure_Id)
                       ? ChordManager.GetProportionalLocationX(prevCh, ratio)
                       : maxStChInMg.Location_X + 25;
        }

        private int? GetLocXByExistingChSt(Repository.DataService.Measure activeChM, double? activeChSt)
        {
            if (_chordProjections == null) return null;
            var q = (from b in _chordProjections where b.NormalizedStarttime == activeChSt select b.LocationX);
            var e = q as IList<int> ?? q.ToList();
            return e.Any() ? e.First() : (int?)null;
        }

        private int? GetLocXFromAdjacentChs(Repository.DataService.Measure activeM, double? activeChSt, double ratio)
        {
            if (activeChSt == null) return null;
            _spacer = new SpacingHelper(activeM, activeChSt);
            foreach (var ch in _chordProjections)
            {
                if (ch.NormalizedStarttime < activeChSt)
                {
                    _spacer.LeftChord = Utils.GetChord(ch.Id);
                    break;
                }
                _spacer.RightChord = Utils.GetChord(ch.Id); ;
            }
            if (_spacer.LeftChord == null || _spacer.RightChord == null) return null;
            return ChordManager.GetAveragedLocationX(_spacer.LeftChord, _spacer.RightChord, ratio);
        }

        private static int GetChordStarttime(Chord activeCh)
        {
            // the starttime of the first chord in a measure is the starttime of the measure
            var m = Utils.GetMeasure(activeCh.Measure_Id);
            var mStaffgroup = Utils.GetStaffgroup(m);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            return ((m.Index % mDensity) * DurationManager.Bpm) + (mStaffgroup.Index * mDensity * DurationManager.Bpm);
        }

        private static double? GetChordStarttime(Chord prevCh, Chord activeCh)
        {
            // the starttime of all chords after the first chord in a measure is
            // the starttime of the previous chord plus the duration of the chord
            return prevCh.StartTime + (double)prevCh.Duration;
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

        public void Select(Note n)
        {
            if (n == null) return;

            var status = Collaborations.GetStatus(n);
            if (status != null)
            {
                //TODO: Isn't there a method to accomplish this conditional evaluation? what is this conditional about?
                if (status == (int)_Enum.Status.AuthorOriginal ||
                    status == (int)_Enum.Status.ContributorAdded ||
                    status == (int)_Enum.Status.AuthorAdded)
                {
                    if (!EditorState.DoubleClick) return;
                    EditorState.DoubleClick = false;
                    var ng = NotegroupManager.ParseChord(Chord, n);
                    foreach (var g in ng.Notes)
                    {
                        EA.GetEvent<SelectNote>().Publish(g.Id);
                    }
                }
            }
        }
    }
}