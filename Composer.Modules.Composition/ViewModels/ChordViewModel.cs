using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Measure = Composer.Infrastructure.Constants.Measure;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels
{
    // TODO: remove _chord.Location_Y from schema
    public sealed class ChordViewModel : BaseViewModel, IChordViewModel
    {
        private int _adjustedLocationX;
        private Chord _chord;

        public ChordViewModel(string id)
        {
            Debug.WriteLine("Entry: ChordViewModel");
            _chord = null;
            HideSelector();
            Chord = Utils.GetChord(Guid.Parse(id));
            if (!EditorState.IsComposing)
            {
                EA.GetEvent<EditorStateChanged>().Publish(true);
            }
            DefineCommands();
            SubscribeEvents();
            SetRepository();
            EA.GetEvent<NotifyChord>().Publish(Chord.Measure_Id);
            EA.GetEvent<AdjustAppendSpace>().Publish(Chord.Measure_Id);
        }

        public Chord Chord
        {
            get { return _chord; }
            set
            {
                _chord = value;
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
            EA.GetEvent<DeleteChord>().Subscribe(OnDelete);
            EA.GetEvent<ChordClicked>().Subscribe(OnChildClick, true);
            EA.GetEvent<SetChordLocationAndStarttime>().Subscribe(OnSetChordLocationAndStarttime);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
        }

        public void OnSynchronizeChord(Chord ch)
        {
            //when the ch_st or location of a ch changes, then it's constituent ns must be synchronized with the ch. 
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
                //if isCollaboration, and all notes in the chord are inactive, then start the
                //flow that replaces the chord with a rest.
                if (!CollaborationManager.IsActive(ch))
                {
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    rest = NoteController.Create(ch, m);
                    rest = NoteController.Deactivate(rest);
                    rest.Pitch = Defaults.RestSymbol;
                    rest.Location_X = ch.Location_X;

                    //the n is already deleted marked as purged. we just need to determine the appropriate status for the n.
                    //if the deleted n was purge-able (see NoteController) then it was deleted from the DB and the n status
                    //is set as if it was a normal add to the m.
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
                        //if note was not purgeable (see NoteController) it must be retained with it's status marked WaitingOn....
                        //the actual status won't be resolved until the note author chooses to reject or accept the note deletion.

                        //another way to say it: the logged in user deleted this note. it's the last note in the chord so the chord is 
                        //replaced by a rest but we can't delete the note because the other collaborartor may not want to accept 
                        //the delete. so there is a rest and a chord occupying the same starttime. if the collaborartor accepts 
                        //the delete, the note can be purged and the rest has its status set appropriately. if the delete is 
                        //rejected, both remain at the same starttime and the rest has its status set appropriately (see NoteViewModel.OnRejectChange)

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
            EA.GetEvent<SpanMeasure>().Publish(m);
        }

        private DataServiceRepository<Repository.DataService.Composition> _repository;
        public void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
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

        public void OnSetChordLocationAndStarttime(Tuple<Guid, Guid, double> payload)
        {
            // when not collaborating, the location_x in the database, IS the Location_X of the chord.
            // when collaborating, the location_x is variable and depends on whether a collaborator is selected,
            // and what contributions have been accepted and/or rejected by the current user.
            Debug.WriteLine("Entry: OnSetChordLocationAndStarttime");
            var chId1 = payload.Item2;
            var chId2 = payload.Item1;
            var mWidthRatio = payload.Item3;

            if (chId2 != Chord.Id) return;
            var ch2 = Utils.GetChord(chId2);
            var m = Utils.GetMeasure(ch2.Measure_Id);

            if (chId1 != Guid.Empty)
            {
                var ch1 = Utils.GetChord(chId1);
                double spacing = DurationManager.GetProportionalSpace((double)ch2.Duration);
                spacing = spacing*mWidthRatio;
                AdjustedLocationX = (int)(Math.Ceiling(ch1.Location_X + spacing));
                ch2.StartTime = GetChordStarttime(ch1, ch2);
				Debug.WriteLine("In: OnSetChordLocationAndStarttime  Going to: SynchronizeSequenceMeasures");
                AdjustedLocationX = SynchronizeSequenceMeasures(ch1.Measure_Id, ch2.StartTime, AdjustedLocationX);
            }
            else
            {
                ch2.StartTime = GetChordStarttime(ch2);
                AdjustedLocationX = Measure.Padding;
            }
        }

        private int SynchronizeSequenceMeasures(Guid mId, double? st, int x)
        {
            var srcM = Utils.GetMeasure(mId);
            var sequence = Utils.GetMeasure(mId).Sequence;
	        var m = Utils.GetMaxChordCountMeasureBySequence(sequence);
            if (m == null) return x;
            if (mId == m.Id) return x;
            if (!MeasureManager.IsPacked(m)) return x;
            var ch = Utils.GetChordByStarttime(m.Id, st);
            if (ch == null) return x;
            Debug.WriteLine("curr_m {0} target_m {1}  st {2}  x {3} x' {4}", srcM.Index, m.Index, st, x, ch.Location_X);
            return ch.Location_X;
        }

        private static int GetChordStarttime(Chord ch2)
        {
            // the starttime of the first chord in a measure is the starttime of the measure
            var m = Utils.GetMeasure(ch2.Measure_Id);
            var mStaffgroup = Utils.GetStaffgroup(m);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mStarttime = ((m.Index % mDensity) * DurationManager.Bpm) + (mStaffgroup.Index * mDensity * DurationManager.Bpm);
            return mStarttime;
        }

        private static double? GetChordStarttime(Chord ch1, Chord ch2)
        {
            // the starttime of all chords after the first chord in a measure is
            // the starttime of the previous chord plus the duration of the chord
            return ch1.StartTime + (double)ch1.Duration;
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