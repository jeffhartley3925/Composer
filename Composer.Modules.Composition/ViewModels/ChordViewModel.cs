using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
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
        public ObservableCollection<Chord> ActiveChords;
        private int _adjustedLocationX;
        private Chord _chord;
        private double _spacing;
        private SpacingHelper _spacingHelper = null;
        private IEnumerable<Chord> activeSeqChs;

        public ChordViewModel(string id)
        {
            _chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(id));
            var m = Utils.GetMeasure(Chord.Measure_Id);
            if (!EditorState.IsComposing)
            {
                EA.GetEvent<EditorStateChanged>().Publish(true);
            }
            DefineCommands();
            SubscribeEvents();
            SetRepository();
            EA.GetEvent<NotifyChord>().Publish(Chord.Measure_Id);
            if (!EditorState.IsOpening)
            {
                EA.GetEvent<BumpMeasureWidth>().Publish(new Tuple<Guid, double, int>(Chord.Measure_Id, Preferences.M_END_SPC, m.Sequence));
            }
        }

        public Chord Chord
        {
            get { return _chord; }
            set
            {
                _chord = value;
                var index = Utils.GetMeasure(Chord.Measure_Id);
                AdjustedLocationX = _chord.Location_X;
                OnPropertyChanged(() => Chord);
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
                    var index = Utils.GetMeasure(Chord.Measure_Id).Index;
                    //EA.GetEvent<SynchronizeChord>().Publish(Chord);
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
            EA.GetEvent<DeleteChord>().Subscribe(OnDelete);
            EA.GetEvent<ChordClicked>().Subscribe(OnChildClick, true);
            EA.GetEvent<SetChordLocationAndStarttime>().Subscribe(OnSetChordLocationXAndStarttime);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
        }

        public void OnSetThreshholdStarttime(Tuple<Guid, double> payload)
        {

        }

        public void OnSynchronizeChord(Chord ch)
        {
            if (ch.Id != Chord.Id) return;
            var index = Utils.GetMeasure(ch.Measure_Id).Index;
            var ns = ChordManager.GetActiveNotes(ch.Notes);
            foreach (var n in ns)
            {
                n.StartTime = ch.StartTime;
                n.Location_X = ch.Location_X;
                EA.GetEvent<UpdateNote>().Publish(n);
                _repository.Update(n);
            }
            EA.GetEvent<UpdateChord>().Publish(ch);
        }

        public void Delete(Chord ch)
        {
            // the only way a chord can be deleted is by deleting all of it's notes first. so, every time a note is deleted, this method
            // is called to check and see if the parent chord should be deleted (by replacing the note with a rest.)
            var m = Utils.GetMeasure(Chord.Measure_Id);
            Note rest;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last note in the chord, and the composition is not under collaboration
                // then delete the chord from the DB and insert a rest in it's place.
                if (Chord.Notes.Count == 0)
                {
                    // add a rest to the empty chord
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    rest = NoteController.Create(ch, m);
                    rest = NoteController.Deactivate(rest);
                    rest.Pitch = Defaults.RestSymbol;
                    rest.Location_X = ch.Location_X;
                    Cache.Notes.Add(rest);
                    ch.Notes.Add(rest);
                    _repository.Update(ch);
                }
            }
            else
            {
                // if isCollaboration, and all notes in the chord are inactive, then start the
                // flow that replaces the chord with a rest.
                if (!CollaborationManager.IsActive(ch))
                {
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    rest = NoteController.Create(ch, m);
                    rest = NoteController.Deactivate(rest);
                    rest.Pitch = Defaults.RestSymbol;
                    rest.Location_X = ch.Location_X;

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

        private DataServiceRepository<Repository.DataService.Composition> _repository;
        public void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        public void OnDelete(Chord chord)
        {
            if (chord.Id != Chord.Id) return;
            Delete(chord);
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

        public void OnSetChordLocationXAndStarttime(Tuple<Guid, Guid, double> payload)
        {
            var prevChId = payload.Item1;
            var activeChId = payload.Item2;
            var ratio = payload.Item3;
            if (prevChId != Chord.Id) return;
            var activeCh = Utils.GetChord(prevChId);
            var m = Utils.GetMeasure(activeCh.Measure_Id);
            if (activeChId != Guid.Empty)
            {
                var prevCh = Utils.GetChord(activeChId);

                AdjustedLocationX = ChordManager.GetProportionalLocationX(prevCh, ratio);
                activeCh.StartTime = GetChordStarttime(prevCh, activeCh);
                _spacingHelper = null;
                var amidx = Utils.GetMeasure(activeCh.Measure_Id).Index;
                var chX = GetLocationX(prevCh, activeCh, AdjustedLocationX, ratio);
                if (chX != null)
                {
                    AdjustedLocationX = (int)chX;
                    if (_spacingHelper == null) return;
                    var shiftCompleted = false;
                    if (_spacingHelper.LeftChord != null)
                    {
                        if (_spacingHelper.LeftChord.Location_X >= activeCh.Location_X &&
                            activeCh.StartTime > _spacingHelper.LeftChord.StartTime)
                        {
                            //back shift
                            var startX = (int) Math.Ceiling((activeCh.Location_X - (activeCh.Location_X - prevCh.Location_X)/2));
                            EA.GetEvent<ShiftChords>().Publish(new Tuple<Guid, int, double, int>(_spacingHelper.ActiveMeasure.Id, _spacingHelper.Sequence, (double)_spacingHelper.LeftChord.StartTime, startX));
                            shiftCompleted = true;
                        }
                    }
                    if (shiftCompleted) return;
                    //forward shift
                    var spacing = ChordManager.GetProportionalSpacing(prevCh, ratio);
                    EA.GetEvent<ShiftChords>()
                      .Publish(new Tuple<Guid, int, double, int>(_spacingHelper.ActiveMeasure.Id,
                          _spacingHelper.Sequence, (double)_spacingHelper.Starttime, activeCh.Location_X + spacing));
                }
            }
            else
            {
                activeCh.StartTime = GetChordStarttime(activeCh);
                AdjustedLocationX = Measure.Padding;
            }
        }

        private Chord GetLowestDurationChord(Chord prevCh, Chord activeCh)
        {
            if (activeCh == null) return prevCh;
            return (prevCh.Duration < activeCh.Duration) ? prevCh : activeCh;
        }

        private int? GetLocationX(Chord prevCh, Chord activeCh, int defaultX, double ratio)
        {
            if (EditorState.IsOpening) return defaultX;
            double? activeChSt = activeCh.StartTime;
            if (activeChSt == null) return defaultX;
            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple)
            {
                //return defaultX;
            }
            var activeM = Utils.GetMeasure(prevCh.Measure_Id);
            var activeSeq = activeM.Sequence;
            var maxStChInSeq = Utils.GetMaxStarttimeChordInSequence(activeSeq, activeM.Id, activeCh.Id);
            if (activeM.Index == 0)
            {
                return null;
            }
            //return defaultX;
            var chX = GetLocationXBySequenceStarttime(activeSeq, activeChSt, activeM.Id);
            if (chX != null) return chX;
            chX = GetLocationXByAdjacentChords(prevCh, activeM, activeChSt, ratio);
            if (chX != null) return chX;
            chX = GetLocationXByLastChordInSequence(ratio, GetLowestDurationChord(prevCh, maxStChInSeq), prevCh, activeCh);
            return chX;
        }

        private int? GetLocationXByLastChordInSequence(double ratio, Chord maxStChInSeq, Chord prevCh, Chord activeCh)
        {
            return (maxStChInSeq.Measure_Id == activeCh.Measure_Id)
                       ? ChordManager.GetProportionalLocationX(prevCh, ratio)
                       : maxStChInSeq.Location_X + 25;
        }

        private int? GetLocationXBySequenceStarttime(int sequence, double? activeChSt, Guid activeMId)
        {
            activeSeqChs = Utils.GetActiveChordsBySequence(sequence, activeMId);
            var seqStsAndLocXs = (from a in activeSeqChs select new { a.StartTime, a.Location_X }).ToList().Distinct();
            var q = (from b in seqStsAndLocXs where b.StartTime == activeChSt select b.Location_X);
            var e = q as IList<int> ?? q.ToList();
            if (e.Any())
            {
                return e.First();
            }
            return null;
        }

        private int? GetLocationXByAdjacentChords(Chord prevCh, Repository.DataService.Measure activeM, double? activeChSt, double ratio)
        {
            if (activeChSt == null) return null;
            _spacingHelper = new SpacingHelper(activeM, (double) activeChSt);
            activeSeqChs = activeSeqChs.OrderByDescending(p => p.StartTime);
            foreach (var ch in activeSeqChs)
            {
                if (ch.StartTime < activeChSt)
                {
                    _spacingHelper.LeftChord = ch;
                    break;
                }
                _spacingHelper.RightChord = ch;
            }
            if (_spacingHelper.LeftChord == null || _spacingHelper.RightChord == null) return null;
            return ChordManager.GetProportionalLocationX(prevCh, ratio);
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