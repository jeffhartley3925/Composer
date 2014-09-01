using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels
{
    // TODO: remove _chord.Location_Y from schema
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel, IEventCatcher
    {
        private DataServiceRepository<Repository.DataService.Composition> repository;

        private int adjustedLocationX;

		private Measuregroup mG;

		public Measuregroup Mg
		{
			get
			{
				if (this.mG == null)
				{
					if (MeasuregroupManager.CompMgs != null)
					{
						var b = (from a in MeasuregroupManager.CompMgs where a.Measures.Contains(Measure) select a);
						if (b.Any())
						{
							this.mG = b.First();
						}
					}
				}
				return this.mG;
			}
			set
			{
				this.mG = value;
			}
		}

		private Repository.DataService.Measure measure;

		public Repository.DataService.Measure Measure
		{
			get
			{
				if (this.measure == null && Chord != null)
				{
					this.measure = Utils.GetMeasure(Chord.Measure_Id);
				}
				return this.measure;
			}
			set
			{
				this.measure = value;
			}
		}

        public ChordViewModel(string iD)
        {
            Chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(iD));
	        if (Chord == null) return;
            SetRepository();
            SubscribeEvents();
            DefineCommands();
            if (!EditorState.IsOpening)
            {
	            if (this.Mg != null)
	            {
		            EA.GetEvent<RespaceMeasuregroup>().Publish(this.Mg.Id);
	            }
	            EA.GetEvent<BumpSequenceWidth>().Publish(new Tuple<Guid, double?, int>(Chord.Measure_Id, null, Measure.Sequence));
				EA.GetEvent<SetCompositionWidth>().Publish(Measure.Staff_Id);
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

        private Chord chord;

        public Chord Chord
        {
            get { return this.chord; }
            set
            {
                this.chord = value;
                if (this.chord != null)
                {
                    AdjustedLocationX = this.chord.Location_X;
                    OnPropertyChanged(() => Chord);
                }

            }
        }

        public int AdjustedLocationX
        {
            get { return this.adjustedLocationX; }
            set
            {
                if (value != this.adjustedLocationX)
                {
                    this.adjustedLocationX = value;
                    Chord.Location_X = this.adjustedLocationX;
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
            EA.GetEvent<SetChordLocationX>().Subscribe(OnSetChordLocationX);
        }

        public void OnSetChordLocationX(Tuple<Guid, int, double> payload)
        {
	        var sT = payload.Item3;
            if (IsTargetVM(sT))
            {
                AdjustedLocationX = payload.Item2;
                Chord.Location_X = AdjustedLocationX;
                EA.GetEvent<SynchronizeChord>().Publish(Chord);
            }
        }

        public void OnSynchronizeChord(Chord cH)
        {
            // when the chord starttime or location of a ch changes, then it's constituent notes must be synchronized with the ch. 
            if (cH.Id != Chord.Id) return;
            var nTs = ChordManager.GetActiveNotes(cH.Notes);
            foreach (var nT in nTs)
            {
                if (nT.StartTime == cH.StartTime && nT.Location_X == cH.Location_X) continue;
                nT.StartTime = cH.StartTime;
                nT.Location_X = cH.Location_X;
                EA.GetEvent<UpdateNote>().Publish(nT);
                this.repository.Update(nT);
            }
            EA.GetEvent<UpdateChord>().Publish(cH);
        }

        public void SetRepository()
        {
            if (this.repository == null)
            {
                this.repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }
        }

        public void OnDeleteChord(Chord cH)
        {
            if (cH.Id != Chord.Id) return;
            Note rE;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last note in the chord, and the composition is not under collaboration
                // then delete the chord from the DB and insert a rest in it's place.
                if (Chord.Notes.Count == 0)
                {
                    Measure.Chords.Remove(cH);
                    Cache.Chords.Remove(cH);
                    this.repository.Delete(cH);

					rE = InsertRest(cH);
                    if (rE == null)
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
					rE = InsertRest(cH);
                    if (rE == null)
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
                            rE.Status = Collaborations.SetStatus(rE, (int)_Enum.Status.AuthorAdded);
                            rE.Status = Collaborations.SetAuthorStatus(rE, (int)_Enum.Status.AuthorOriginal);
                        }
                        else
                        {
                            rE.Status = Collaborations.SetStatus(rE, (int)_Enum.Status.ContributorAdded, Collaborations.Index);
                            rE.Status = Collaborations.SetAuthorStatus(rE, (int)_Enum.Status.PendingAuthorAction);
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

                        rE.Status = (EditorState.EditContext == (int)_Enum.EditContext.Authoring) ?
                            Collaborations.SetStatus(rE, (short)_Enum.Status.WaitingOnContributor, 0) :
                            Collaborations.SetStatus(rE, (short)_Enum.Status.WaitingOnAuthor, Collaborations.Index);
                    }
                    cH.Notes.Add(rE);
                    this.repository.Update(cH);
                }
            }
            EA.GetEvent<DeleteTrailingRests>().Publish(string.Empty);
			var chords = ChordManager.GetActiveChords(Measure.Chords);
            if (chords.Count <= 0) return;
			EA.GetEvent<SpanMeasure>().Publish(Measure.Id);
        }

        private Note InsertRest(Chord source)
        {
            EditorState.Duration = (double)source.Duration;
            EditorState.SetRestContext();
            if (source.StartTime != null)
            {
				var cH = ChordManager.AddChord(Measure.Id, (double)source.StartTime);
                cH.Location_X = source.Location_X;
				var rE = NoteController.Create(cH, Measure);
                rE = NoteController.Activate(rE);
                rE.Pitch = Defaults.RestSymbol;
                rE.Location_X = source.Location_X;
                rE.Location_Y = Finetune.Measure.RestLocationY;
				Cache.AddNote(rE);
                cH.Notes.Add(rE);
                this.repository.Update(rE);
                Cache.Chords.Add(cH);
				Measure.Chords.Add(cH);
                this.repository.Update(cH);
				this.repository.Update(Measure);
                EA.GetEvent<SynchronizeChord>().Publish(cH);
                return rE;
            }
            return null;
        }

        public override void OnClick(object obj)
        {
            var note = (Note)obj;
            Select(note);
        }

        public void OnUpdateChord(Chord cH)
        {
            if (cH.Id != Chord.Id) return;
            Chord = cH;
        }

        public override void OnChildClick(object obj)
        {
            var nT = (Note)obj;
            if (Chord.StartTime.ToString() == nT.StartTime.ToString())
            {
                OnClick(nT);
            }
        }

        private Chord GetLowestDurationChord(Chord prevCh, Chord currCh)
        {
            if (currCh == null) return prevCh;
            return (prevCh.Duration < currCh.Duration) ? prevCh : currCh;
        }

	    public void OnDeSelectComposition(object obj)
        {
            if (IsSelected)
            {
                EA.GetEvent<DeSelectChord>().Publish(Chord.Id);
            }
        }

        public void OnSelectChord(Guid iD)
        {
            if (Chord.Id != iD) return;
            foreach (var nT in Chord.Notes)
            {
                if (CollaborationManager.IsActive(nT))
                {
                    EA.GetEvent<SelectNote>().Publish(nT.Id);
                }
            }
            ShowSelector();
        }

        public void OnDeSelectChord(Guid iD)
        {
            if (Chord.Id != iD) return;
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