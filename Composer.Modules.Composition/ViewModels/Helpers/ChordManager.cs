using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;

namespace Composer.Modules.Composition.ViewModels
{
    public static class ChordManager
    {
        public static Guid SelectedChordId = Guid.Empty;
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        public static ChordViewModel Vm { get; set; }
        public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
        public static decimal[] ChordStartTimes;
        public static decimal[] ChordInactiveTimes;
        public static Chord Chord { get; set; }
        public static List<Notegroup> ChordNotegroups { get; set; }
        public static Repository.DataService.Measure Measure { get; set; }
        public static int Location_X { get; set; }
        public static int Location_Y { get; set; }
        private static IEventAggregator _ea;
        public static List<Guid> InertChords = new List<Guid>();
        public static ObservableCollection<Chord> ActiveChords;

        static ChordManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        private static void SubscribeEvents()
        {
            _ea.GetEvent<SynchronizeChord>().Subscribe(OnSynchronize);
        }

        public static ObservableCollection<Chord> GetActiveChords(Repository.DataService.Measure m)
        {
            return GetActiveChords(m.Chords, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static ObservableCollection<Chord> GetActiveChords(Repository.DataService.Measure m, Collaborator collaborator)
        {
            return GetActiveChords(m.Chords, collaborator);
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chs)
        {
            return GetActiveChords(chs, CollaborationManager.GetCurrentAsCollaborator());
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chs, Collaborator collaborator)
        {
            return new ObservableCollection<Chord>((
                from ch in chs
                where CollaborationManager.IsActive(ch, collaborator)
                select ch).OrderBy(p => p.StartTime));
        }

        public static ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> ns)
        {
            return new ObservableCollection<Note>((
                from n in ns
                where CollaborationManager.IsActive(n)
                select n).OrderBy(p => p.StartTime));
        }

        public static decimal SetDuration(Chord ch)
        {
            var n = GetActiveNotes(ch.Notes);
            var a = (from c in n select c.Duration);
            var e = a as List<decimal> ?? a.ToList();
            return (!e.Any()) ? ch.Duration : e.Min();
        }

        public static double GetChordStarttime(double mStarttime)
        {
            var mDuration = Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
            return mDuration + mStarttime;
        }
        public static Chord GetOrCreate(Guid pId)
        {
            //if click is in-line with an existing ch, return that ch. otherwise create and return a new ch
            Chord ch;
            if (EditorState.Chord != null)
            {
                //existing active ch
                //the ch d is set to the minimum d of its ns.
                if (EditorState.Duration != null && (decimal)EditorState.Duration < EditorState.Chord.Duration)
                {
                    EditorState.Chord.Duration = (decimal)EditorState.Duration;
                }
                return EditorState.Chord;
            }
            var mStaffgroup = Utils.GetStaffgroup(Measure);
            var mDensity = Infrastructure.Support.Densities.MeasureDensity;
            var mStarttime = ((Measure.Index % mDensity) * DurationManager.Bpm) + (mStaffgroup.Index * mDensity * DurationManager.Bpm); //TODO: this can move out of here, since its a constant.
            
            var chStarttime = GetChordStarttime(mStarttime);
            //what if there's an inactive ch (therefore, not visible) with the same st?
            var a = (from b in Cache.Chords where b.StartTime == chStarttime && EditorState.ActiveMeasureId == b.Measure_Id select b);
            var e = a as List<Chord> ?? a.ToList();
            if (e.Any())
            {
                //here the ch exists, but it's inactive. return it.
                ch = e.First();
                EditorState.Chord = ch;
                if (EditorState.Duration != null) ch.Duration = (decimal)EditorState.Duration;
            }
            else
            {
                //brand new ch. create and return
                ch = _repository.Create<Chord>();
                ch.Id = Guid.NewGuid();
                if (EditorState.Duration != null) ch.Duration = (decimal)EditorState.Duration;
                ch.StartTime = chStarttime;
                ch.Measure_Id = pId;
                ch.Audit = GetAudit();
                ch.Status = CollaborationManager.GetBaseStatus();
            }
            return ch;
        }


        public static Audit GetAudit()
        {
            var audit = new Audit
            {
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                Author_Id = Current.User.Id,
                CollaboratorIndex = 0
            };
            return audit;
        }


        public static Chord Clone(Repository.DataService.Measure m, Chord ch, Collaborator col)
        {
            Chord o;
            Measure = m;
            EditorState.Duration = (double)ch.Duration;
            o = GetOrCreate(m.Id);
            o.Id = Guid.NewGuid();
            o.Measure_Id = m.Id;
            o.Duration = ch.Duration;
            o.Key_Id = ch.Key_Id;
            o.Location_X = ch.Location_X;
            o.Location_Y = ch.Location_Y;
            o.Audit = GetAudit();
            o.StartTime = ch.StartTime;
            o.Status = CollaborationManager.GetBaseStatus();
            var idx = 0;
            foreach (var n in ch.Notes.Select(note => NoteController.Clone(o.Id, ch, Measure, o.Location_X + (idx * 16), note.Location_Y, note, col)))
            {
                o.Notes.Add(n);
                idx++;
            }
            Cache.Chords.Add(o);
            InertChords.Add(o.Id);
            return o;
        }

        public static Chord Clone(Repository.DataService.Measure m, Chord ch)
        {
            Chord o = null;
            try
            {
                Measure = m;
                EditorState.Duration = (double)ch.Duration;
                if (ch.StartTime != null)
                {
                    o = GetOrCreate(m.Id);
                    o.Id = Guid.NewGuid();
                    o.Measure_Id = m.Id;
                    o.Duration = ch.Duration;
                    o.Key_Id = ch.Key_Id;
                    o.Location_X = ch.Location_X;
                    o.Location_Y = ch.Location_Y;
                    o.Audit = GetAudit();
                    o.StartTime = ch.StartTime;
                    o.Status = CollaborationManager.GetBaseStatus();
                    var idx = 0;
                    foreach (var sourceNote in ch.Notes.Select(n => NoteController.Clone(o.Id, ch, Measure, o.Location_X + (idx * 16), n.Location_Y, n)))
                    {
                        o.Notes.Add(sourceNote);
                        idx++;
                    }
                    Cache.Chords.Add(o);
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return o;
        }

        public static void Delete(Chord ch)
        {
            // the only way a chord can be deleted is by deleting all of it's notes first. so, every time a note is deleted, this method
            // is called to check and see if the underlying parent ch should be deleted. if so, it is pseudo-deleted by adding a note to the chord.
            var m = Utils.GetMeasure(Vm.Chord.Measure_Id);
            Note n;
            if (!EditorState.IsCollaboration)
            {
                // if we are deleting the last n (or the only n) in the ch, and the composition is not under collaboration
                // then delete the ch from the DB and insert a n in it's place.
                if (Vm.Chord.Notes.Count == 0)
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
            _ea.GetEvent<DeleteTrailingRests>().Publish(string.Empty);
            var chords = GetActiveChords(m.Chords);
            if (chords.Count <= 0) return;
            _ea.GetEvent<UpdateSpanManager>().Publish(m.Id);
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            _ea.GetEvent<SpanMeasure>().Publish(m);
        }

        public static void OnSynchronize(Chord ch)
        {
            //when the ch_st or location of a ch changes, then it's constituent ns must be synchronized with the ch. 
            var ns = GetActiveNotes(ch.Notes);
            foreach (var n in ns)
            {
                if (n.StartTime == ch.StartTime && n.Location_X == ch.Location_X) continue;
                n.StartTime = ch.StartTime;
                n.Location_X = ch.Location_X;
                _ea.GetEvent<UpdateChord>().Publish(ch);
                _ea.GetEvent<UpdateNote>().Publish(n);
                _repository.Update(n);
            }
        }

        public static void Select(Note n)
        {
            if (n == null) return;

            var status = Collaborations.GetStatus(n);
            if (status != null)
            {
                //TODO: Isn't there a method to accomplish this conditional evaluation? what is this conditional about?
                if (status == (int) _Enum.Status.AuthorOriginal ||
                    status == (int) _Enum.Status.ContributorAdded ||
                    status == (int) _Enum.Status.AuthorAdded)
                {
                    if (!EditorState.DoubleClick) return;
                    EditorState.DoubleClick = false;
                    var ng = NotegroupManager.ParseChord(Vm.Chord, n);
                    foreach (var g in ng.Notes)
                    {
                        _ea.GetEvent<SelectNote>().Publish(g.Id);
                    }
                }
            }
        }
    }
}