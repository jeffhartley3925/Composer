using System;
using System.Collections.ObjectModel;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
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
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel, IEventCatcher
    {
        private DataServiceRepository<Repository.DataService.Composition> _repository;

        private int _adjustedLocationX;
        private SpacingHelper _spacer;
        public Chord LastMeasureGroupChord { get; set; }
        public Chord LastSequenceChord { get; set; }
        private IEnumerable<Chord> _activeMeasureGroupChords;
        private Guid _measureGroupId = Guid.Empty;

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

		private Measuregroup _measureGroup = null;
		public Measuregroup Measuregroup
		{
			get
			{
				if (_measureGroup == null)
				{
					if (MeasuregroupManager.CompMgs != null)
					{
						var b = (from a in MeasuregroupManager.CompMgs where a.Measures.Contains(Measure) select a);
						if (b.Any())
						{
							_measureGroup = b.First();
						}
					}
				}
				return _measureGroup;
			}
			set
			{
				_measureGroup = value;
			}
		}

		private Repository.DataService.Measure _measure = null;
		public Repository.DataService.Measure Measure
		{
			get
			{
				if (_measure == null && Chord != null)
				{
					_measure = Utils.GetMeasure(Chord.Measure_Id);
				}
				return _measure;
			}
			set
			{
				_measure = value;
			}
		}

        public ChordViewModel(string id)
        {
            Chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(id));
            SetRepository();
            SubscribeEvents();
            DefineCommands();
            if (!EditorState.IsOpening)
            {
				if (Measuregroup != null)
					EA.GetEvent<RespaceMeasuregroup>().Publish(Measuregroup.Id);
                EA.GetEvent<BumpMeasureWidth>().Publish(new Tuple<Guid, double, int>(Chord.Measure_Id, Preferences.M_END_SPC, Measure.Sequence));
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
            EA.GetEvent<SynchronizeChord>().Subscribe(OnSynchronizeChord);
            EA.GetEvent<DeleteChord>().Subscribe(OnDeleteChord);
            EA.GetEvent<ChordClicked>().Subscribe(OnChildClick, true);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
            EA.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
            EA.GetEvent<SetChordLocationX>().Subscribe(OnSetChordLocationX);
        }

        public void OnSetChordLocationX(Tuple<Guid, int, double> payload)
        {
            var cHiD = payload.Item1;
            var sT = payload.Item3;
            if (IsTargetVM(sT))
            {
                AdjustedLocationX = payload.Item2;
                Chord.Location_X = AdjustedLocationX;
                EA.GetEvent<SynchronizeChord>().Publish(Chord);
            }
        }

        public void OnNotifyActiveChords(Tuple<Guid, object, object, object, int, Guid> payload)
        {
            var id = payload.Item1;
            if (id != Chord.Measure_Id) return;
            _measureGroupId = payload.Item6;
            ActiveChords = (ObservableCollection<Chord>)payload.Item2;
            ActiveSequenceChords = (ObservableCollection<Chord>)payload.Item3;
            ActiveMeasureGroupChords = (ObservableCollection<Chord>)payload.Item4;
            LastSequenceChord = (from c in ActiveSequenceChords select c).Last();
            LastMeasureGroupChord = (from c in ActiveMeasureGroupChords select c).Last();
        }

        public void OnSynchronizeChord(Chord cH)
        {
            // when the chord starttime or location of a ch changes, then it's constituent notes must be synchronized with the ch. 
            if (cH.Id != Chord.Id) return;
            var ns = ChordManager.GetActiveNotes(cH.Notes);
            foreach (var nT in ns)
            {
                if (nT.StartTime == cH.StartTime && nT.Location_X == cH.Location_X) continue;
                nT.StartTime = cH.StartTime;
                nT.Location_X = cH.Location_X;
                EA.GetEvent<UpdateNote>().Publish(nT);
                _repository.Update(nT);
            }
            EA.GetEvent<UpdateChord>().Publish(cH);
        }

        public void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        public void OnDeleteChord(Chord cH)
        {
            if (Chord == null || cH == null)
            {

            }
            if (cH.Id != Chord.Id) return;
            Note rest;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last note in the chord, and the composition is not under collaboration
                // then delete the chord from the DB and insert a rest in it's place.
                if (Chord.Notes.Count == 0)
                {
                    Measure.Chords.Remove(cH);
                    Cache.Chords.Remove(cH);
                    _repository.Delete(cH);

					rest = InsertRest(cH);
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
                if (!CollaborationManager.IsActive(cH))
                {
					rest = InsertRest(cH);
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
                    cH.Notes.Add(rest);
                    _repository.Update(cH);
                }
            }
            EA.GetEvent<DeleteTrailingRests>().Publish(string.Empty);
			var chords = ChordManager.GetActiveChords(Measure.Chords);
            if (chords.Count <= 0) return;
			EA.GetEvent<SpanMeasure>().Publish(Measure.Id);
        }

        private Note InsertRest(Chord ch)
        {
            EditorState.Duration = (double)ch.Duration;
            EditorState.SetRestContext();
            if (ch.StartTime != null)
            {
				Chord chord = ChordManager.AddChord(Measure.Id, (double)ch.StartTime);
                chord.Location_X = ch.Location_X;
				Note rest = NoteController.Create(chord, Measure);
                rest = NoteController.Activate(rest);
                rest.Pitch = Defaults.RestSymbol;
                rest.Location_X = ch.Location_X;
                rest.Location_Y = Finetune.Measure.RestLocationY;
                Cache.Notes.Add(rest);
                chord.Notes.Add(rest);
                _repository.Update(rest);
                Cache.Chords.Add(chord);
				Measure.Chords.Add(chord);
                _repository.Update(chord);
				_repository.Update(Measure);
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

        private Chord GetLowestDurationChord(Chord prevCh, Chord currCh)
        {
            if (currCh == null) return prevCh;
            return (prevCh.Duration < currCh.Duration) ? prevCh : currCh;
        }

        private static double? GetChordStarttime(Chord prevCh, Chord currCh)
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

            foreach (var nT in Chord.Notes)
            {
                EA.GetEvent<DeSelectNote>().Publish(nT.Id);
            }
            HideSelector();
        }

        public void Select(Note nT)
        {
            if (nT == null) return;
            var status = Collaborations.GetStatus(nT);
            if (status != null)
            {
                //TODO: Isn't there a method to accomplish this conditional evaluation? what is this conditional about?
                if (status == (int)_Enum.Status.AuthorOriginal ||
                    status == (int)_Enum.Status.ContributorAdded ||
                    status == (int)_Enum.Status.AuthorAdded)
                {
                    if (!EditorState.DoubleClick) return;
                    EditorState.DoubleClick = false;
                    var ng = NotegroupManager.ParseChord(Chord, nT);
                    foreach (var g in ng.Notes)
                    {
                        EA.GetEvent<SelectNote>().Publish(g.Id);
                    }
                }
            }
        }

        public bool IsTargetVM(Guid Id)
        {
            return this.Chord.Id == Id;
        }

        public bool IsTargetVM(double sT)
        {
            return this.Chord.StartTime == sT;
        }
    }
}