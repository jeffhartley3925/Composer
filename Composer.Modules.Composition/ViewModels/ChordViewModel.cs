using System;
using System.Collections;
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
            EA.GetEvent<AdjustMeasureEndSpace>().Publish(string.Empty);
            EA.GetEvent<AdjustAppendSpace>().Publish(Chord.Measure_Id);
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
            EA.GetEvent<SetChordLocationAndStarttime>().Subscribe(OnSetChordLocationAndStarttime);
            EA.GetEvent<UpdateChord>().Subscribe(OnUpdateChord);
            EA.GetEvent<SelectChord>().Subscribe(OnSelectChord);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
            EA.GetEvent<DeSelectChord>().Subscribe(OnDeSelectChord);
        }

        #endregion

        public void Delete(Chord ch)
        {
            // the only way a chord can be deleted is by deleting all of it's notes first. so, every time a note is deleted, this method
            // is called to check and see if the underlying parent ch should be deleted. if so, it is pseudo-deleted by adding a note to the chord.
            var m = Utils.GetMeasure(Chord.Measure_Id);
            Note n;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last n (or the only n) in the ch, and the composition is not under collaboration
                // then delete the ch from the DB and insert a n in it's place.
                if (Chord.Notes.Count == 0)
                {
                    //add a n to the empty ch
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    n = NoteController.Create(ch, m);
                    n = NoteController.Deactivate(n);
                    n.Pitch = Defaults.RestSymbol;
                    n.Location_X = ch.Location_X;
                    Cache.Notes.Add(n);
                    ch.Notes.Add(n);
                    _repository.Update(ch);
                }
            }
            else
            {
                //if isCollaboration, and all ns in the ch are inactive, then start the
                //flow that replaces the ch with a n.
                if (!CollaborationManager.IsActive(ch))
                {
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    n = NoteController.Create(ch, m);
                    n = NoteController.Deactivate(n);
                    n.Pitch = Defaults.RestSymbol;
                    n.Location_X = ch.Location_X;

                    //the n is already deleted marked as purged. we just need to determine the appropriate status for the n.
                    //if the deleted n was purge-able (see NoteController) then it was deleted from the DB and the n status
                    //is set as if it was a normal add to the m.
                    if (EditorState.Purgable)
                    {
                        if (EditorState.EditContext == (int)_Enum.EditContext.Authoring)
                        {
                            n.Status = Collaborations.SetStatus(n, (int)_Enum.Status.AuthorAdded);
                            n.Status = Collaborations.SetAuthorStatus(n, (int)_Enum.Status.AuthorOriginal);
                        }
                        else
                        {
                            n.Status = Collaborations.SetStatus(n, (int)_Enum.Status.ContributorAdded, Collaborations.Index);
                            n.Status = Collaborations.SetAuthorStatus(n, (int)_Enum.Status.PendingAuthorAction);
                        }
                        EditorState.Purgable = false;
                    }
                    else
                    {
                        //if n was not purgeable (see NoteController) it must be retained with it's status marked WaitingOn....
                        //the actual status won't be resolved until the n author chooses to reject or accept the n deletion.

                        //another way to say it: the logged in user deleted this n. it's the last n in the ch so the ch is 
                        //replaced by a n but we can't delete the n because the other col may not want to accept 
                        //the delete. so there is a n and a ch occupying the same st. if the col accepts 
                        //the delete, the n can be purged and the n has its status set appropriately. if the delete is 
                        //rejected, both remain at the same st and the n has its staus set appropriately (see NoteViewModel.OnRejectChange)

                        n.Status = (EditorState.EditContext == (int)_Enum.EditContext.Authoring) ?
                            Collaborations.SetStatus(n, (short)_Enum.Status.WaitingOnContributor, 0) :
                            Collaborations.SetStatus(n, (short)_Enum.Status.WaitingOnAuthor, Collaborations.Index); //replaced a hard coded '0' with 'Collaborations.Index' on 9/27/2012
                    }
                    Cache.Notes.Add(n);
                    ch.Notes.Add(n);
                    _repository.Update(ch);
                }
            }
            EA.GetEvent<DeleteTrailingRests>().Publish(string.Empty);
            var chords = ChordManager.GetActiveChords(m.Chords);
            if (chords.Count <= 0) return;
            EA.GetEvent<UpdateSpanManager>().Publish(m.Id);
            //MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            EA.GetEvent<SpanMeasure>().Publish(m);
        }

        public void OnSynchronize(Chord ch)
        {
            //when the ch_st or location of a ch changes, then it's constituent ns must be synchronized with the ch. 
            var ns = ChordManager.GetActiveNotes(ch.Notes);
            foreach (var n in ns)
            {
                if (n.StartTime == ch.StartTime && n.Location_X == ch.Location_X) continue;
                n.StartTime = ch.StartTime;
                n.Location_X = ch.Location_X;
                EA.GetEvent<UpdateChord>().Publish(ch);
                EA.GetEvent<UpdateNote>().Publish(n);
                _repository.Update(n);
            }
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
                AdjustedLocation_X = (int)(Math.Ceiling(ch1.Location_X + spacing));
                ch2.StartTime = GetChordStarttime(ch1, ch2);
                AdjustedLocation_X = (int)SynchronizeSequenceMeasures(ch1.Measure_Id, ch2.StartTime, AdjustedLocation_X);
            }
            else
            {
                ch2.StartTime = GetChordStarttime(ch2);
                AdjustedLocation_X = Measure.Padding;
            }

            EA.GetEvent<SynchronizeChord>().Publish(ch2);
            EA.GetEvent<UpdateChord>().Publish(ch2);
        }

        private int SynchronizeSequenceMeasures(Guid mId, double? st, int x)
        {
            var sequence = Utils.GetMeasure(mId).Sequence;
            var m = Utils.GetMeasureWithMostChordsInSequence(sequence, mId);
            if (m == null) return x;
            if (MeasureManager.IsPacked(m))
            {
                var ch = Utils.GetChordWithStarttime(m.Id, st);
                if (ch != null)
                {
                    return ch.Location_X;
                }
            }
            return x;
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
            return ch1.StartTime + (double)ch2.Duration;
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