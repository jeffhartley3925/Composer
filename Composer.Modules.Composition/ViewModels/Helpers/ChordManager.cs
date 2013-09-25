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
        private static Chord _chord1;
        private static Chord _chord2;
        private static DataServiceRepository<Repository.DataService.Composition> _repo;
        public static ChordViewModel ViewModel { get; set; }
        public static Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
        public static decimal[] ChordStartTimes;
        public static decimal[] ChordInactiveTimes;
        public static Chord Chord { get; set; }
        public static List<Notegroup> ChordNotegroups { get; set; }
        public static Repository.DataService.Measure Measure { get; set; }
        public static int Location_X { get; set; }
        public static int Location_Y { get; set; }
        private static IEventAggregator _ea;
        private static MeasureViewModel _mVm;
        public static List<Guid> InertChords = new List<Guid>();
        public static ObservableCollection<Chord> ActiveChords;
        private static void SetNotegroupContext()
        {
            NotegroupManager.ChordStarttimes = null;
            NotegroupManager.ChordNotegroups = ChordNotegroups;
            NotegroupManager.Measure = Measure;
            NotegroupManager.Chord = Chord;
        }

        static ChordManager()
        {
            Initialize();
        }

        public static void Initialize()
        {
            if (_repo == null)
            {
                _repo = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        private static void SubscribeEvents()
        {
            _ea.GetEvent<SynchronizeChord>().Subscribe(OnSynchronize);
        }

        public static Chord AddNoteToChord(MeasureViewModel mVm)
        {
            _mVm = mVm;
            ActiveChords = mVm.ActiveChords;
            Chord = GetOrCreate(Measure.Id);
            if (Chord != null)
            {
                var n = NoteController.Create(Chord, Measure, Location_X, Location_Y);
                if (n == null) return null;
                Chord.Notes.Add(n);
                Cache.Notes.Add(n);
                SetNotegroupContext();
                ChordNotegroups = NotegroupManager.ParseChord();
                SetNotegroupContext();
                var ng = NotegroupManager.GetNotegroup(n);
                if (ng != null)
                {
                    n.Orientation = ng.Orientation;
                    _ea.GetEvent<FlagNotegroup>().Publish(ng);

                    var ns = GetActiveNotes(Chord.Notes);
                    if (ns.Count == 1)
                    {
                        if (Chord.Notes.Count == 1)
                        {
                            Measure.Chords.Add(Chord);
                            Cache.Chords.Add(Chord);
                        }
                        _ea.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
                        _Enum.NotePlacementMode placementMode = GetNotePlacementMode(out _chord1, out _chord2);
                        Chord.Location_X = GetChordXCoordinate(placementMode, Chord);
                        Measure.Duration = (decimal)Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
                        _repo.Update(Measure);
                    }
                    n.Location_X = Chord.Location_X;
                }
            }
            if (EditorState.IsCollaboration)
            {
                //if this composition has collaborators, then locations and start times need to be adjusted.
                _ea.GetEvent<MeasureLoaded>().Publish(Measure.Id);
            }
            if (Chord != null && Chord.Duration < 1)
            {
                SpanManager.LocalSpans = _mVm.LocalSpans;
                _ea.GetEvent<SpanMeasure>().Publish(Measure);
            }
            _ea.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
            return Chord;
        }

        public static _Enum.NotePlacementMode GetNotePlacementMode(out Chord ch1, out Chord ch2)
        {
            ch1 = null;
            ch2 = null;
            var click_x = Location_X + Finetune.Measure.ClickNormalizer_X;
            var mode = GetBracketChords(out ch1, out ch2, click_x);
            return mode;
        }

        public static _Enum.NotePlacementMode GetBracketChords(out Chord ch1, out Chord ch2, int click_x)
        {
            ch1 = null;
            ch2 = null;
            var loc_x1 = Defaults.MinusInfinity;
            var loc_x2 = Defaults.PlusInfinity;
            var mode = _Enum.NotePlacementMode.Append;

            if (!ActiveChords.Any()) return mode;
            ch1 = ActiveChords[0];
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            for (var i = 0; i < ActiveChords.Count - 1; i++)
            {
                var ach1 = ActiveChords[i];
                var ach2 = ActiveChords[i + 1];

                if (click_x > ach1.Location_X && click_x < ach2.Location_X)
                {
                    ch1 = ach1;
                    ch2 = ach2;
                    mode = _Enum.NotePlacementMode.Insert;
                }
                if (click_x > ach1.Location_X && ach1.Location_X > loc_x1)
                {
                    ch1 = ach1;
                    loc_x1 = ach1.Location_X;
                }
                if (click_x < ach2.Location_X && ach2.Location_X < loc_x2)
                {
                    ch2 = ach2;
                    loc_x2 = ach2.Location_X;
                }
            }
            return mode;
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chs)
        {
            return new ObservableCollection<Chord>((
                from a in chs
                where CollaborationManager.IsActive(a)
                select a).OrderBy(p => p.StartTime));
        }

        //public static ObservableCollection<_chord> GetActiveChordsForSelectedCollaborator(DataServiceCollection<_chord> chs)
        //{
        //    if (chs.Count() == 0) new ObservableCollection<_chord>();
        //    return new ObservableCollection<_chord>((
        //        from a in chs
        //        where CollaborationManager.IsActiveForSelectedCollaborator(a)
        //        select a).OrderBy(p => p.StartTime));
        //}

        public static ObservableCollection<Chord> GetActiveChords(Repository.DataService.Measure m)
        {
            return GetActiveChords(m.Chords);
        }

        //public static ObservableCollection<_chord> GetActiveChordsForSelectedCollaborator(Repository.DataService._measure m)
        //{
        //    return GetActiveChordsForSelectedCollaborator(m.Chords);
        //}
        public static ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> ns)
        {
            return new ObservableCollection<Note>((
                from a in ns
                where CollaborationManager.IsActive(a)
                select a).OrderBy(p => p.StartTime));
        }

        public static decimal SetDuration(Chord ch)
        {
            var n = GetActiveNotes(ch.Notes);
            var a = (from c in n select c.Duration);
            var e = a as List<decimal> ?? a.ToList();
            return (!e.Any()) ? ch.Duration : e.Min();
        }

        private static int GetChordXCoordinate(_Enum.NotePlacementMode mode, Chord ch)
        {
            var loc_x = 0;
            var p_spc = DurationManager.GetProportionalSpace();
            var spc = p_spc;
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            
            switch (mode)
            {
                case _Enum.NotePlacementMode.Insert:
                    if (_chord1 != null && _chord2 != null)
                    {
                        loc_x = _chord1.Location_X + spc;
                        ch.Location_X = loc_x;
                        ch.StartTime = _chord2.StartTime;
                        foreach (Chord ach in ActiveChords)  //no need to filter m.chs using GetActiveChords(). 
                        {
                            if (ach.Location_X > _chord1.Location_X && ch != ach)
                            {
                                ach.Location_X += spc;
                                if (ach.StartTime != null) ach.StartTime = (double)ach.StartTime + (double)ch.Duration;
                                _repo.Update(ach);
                            }
                            _ea.GetEvent<SynchronizeChord>().Publish(ach);
                            _ea.GetEvent<UpdateChord>().Publish(ach);
                        }
                    }
                    break;
                case _Enum.NotePlacementMode.Append:
                    var a = (from c in ActiveChords where c.StartTime < Chord.StartTime select c.Location_X);
                    var e = a as List<int> ?? a.ToList();
                    loc_x = (!e.Any()) ? Infrastructure.Constants.Measure.Padding : Convert.ToInt32(e.Max()) + spc;
                    break;
            }
            return loc_x;
        }

        public static Chord GetOrCreate(Guid pId)
        {
            //if click is inline with an existing ch, return that ch. otherwise create and return a new ch
            Chord ch;
            if (EditorState.Chord != null)
            {
                //existing active ch
                //the ch d is set to the minimum d of its ns.
                if ((decimal)EditorState.Duration < EditorState.Chord.Duration)
                {
                    EditorState.Chord.Duration = (decimal)EditorState.Duration;
                }
                return EditorState.Chord;
            }
            var s = (from h in Cache.Staffs where h.Id == Measure.Staff_Id select h).First();
            var sg = (from p in Cache.Staffgroups where p.Id == s.Staffgroup_Id select p).First();
            var m_dens = Composer.Infrastructure.Support.Densities.MeasureDensity;
            double m_st = ((Measure.Index % m_dens) * DurationManager.BPM) + (sg.Index * m_dens * DurationManager.BPM); //TODO: this can move out of here, since its a constant.
            double ch_st = GetChordStarttime(m_st);
            //what if there's an inactive ch (therefore, noit visible) with the same st?
            var a = (from b in Cache.Chords where b.StartTime == ch_st && EditorState.ActiveMeasureId == b.Measure_Id select b);
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
                ch = _repo.Create<Chord>();
                ch.Id = Guid.NewGuid();
                if (EditorState.Duration != null) ch.Duration = (decimal)EditorState.Duration;
                ch.StartTime = ch_st;
                ch.Measure_Id = pId;
                ch.Audit = GetAudit();
                ch.Status = CollaborationManager.GetBaseStatus();
            }
            return ch;
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
            var index = 0;
            foreach (Note note in ch.Notes)
            {
                Note n = NoteController.Clone(o.Id, ch, Measure, o.Location_X + (index * 16), note.Location_Y, note, col);
                o.Notes.Add(n);
                index++;
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
                    int index = 0;
                    foreach (var note in ch.Notes)
                    {
                        var clonedNote = NoteController.Clone(o.Id, ch, Measure, o.Location_X + (index * 16),
                                                              note.Location_Y, note);
                        o.Notes.Add(clonedNote);
                        index++;
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

        private static Audit GetAudit()
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

        public static void Delete(Chord ch)
        {
            //the only way a ch can be deleted is by deleting all of it's ns first. so, every time a n is deleted, this method
            //is called to check and see if the underlying parent ch should be deleted. if so, it is pseudo-deleted by adding a n to the ch.
            Repository.DataService.Measure m = (from a in Cache.Measures where a.Id == ViewModel.Chord.Measure_Id select a).First();
            Note n;
            if (!EditorState.IsCollaboration)
            {
                //if we are deleting the last n (or the only n) in the ch, and the composition is not under collaboration
                //then delete the ch from the DB and insert a n in it's place.
                if (ViewModel.Chord.Notes.Count == 0)
                {
                    //add a n to the empty ch
                    EditorState.Duration = (double)ch.Duration;
                    EditorState.SetRestContext();
                    n = NoteController.Create(ch, m, ch.Location_X);
                    n = NoteController.Deactivate(n);
                    n.Pitch = Defaults.RestSymbol;
                    n.Location_X = ch.Location_X;
                    Cache.Notes.Add(n);
                    ch.Notes.Add(n);
                    _repo.Update(ch);
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
                    n = NoteController.Create(ch, m, ch.Location_X);
                    n = NoteController.Deactivate(n);
                    n.Pitch = Defaults.RestSymbol;
                    n.Location_X = ch.Location_X;

                    //the n is already deleted marked as purged. we just need to determine the appropriate status for the n.
                    //if the deleted n was purgeable (see NoteController) then it was deleted from the DB and the n status
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
                    _repo.Update(ch);
                }
            }
            _ea.GetEvent<DeleteTrailingRests>().Publish(m.Id);
            var chords = GetActiveChords(m.Chords);
            if (chords.Count > 0)
            {
                _ea.GetEvent<UpdateSpanManager>().Publish(m.Id);
                MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
                _ea.GetEvent<SpanMeasure>().Publish(m);
            }
        }

        private static double GetChordStarttime(double m_st)
        {
            var d = Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
            return d + m_st;
        }

        public static void OnSynchronize(Chord ch)
        {
            //when the ch_st or location of a ch changes, then it's constituent ns must be synchronized with the ch. 
            ObservableCollection<Note> ns = GetActiveNotes(ch.Notes);
            foreach (var n in ns)
            {
                if (n.StartTime != ch.StartTime || n.Location_X != ch.Location_X)
                {
                    n.StartTime = ch.StartTime;
                    n.Location_X = ch.Location_X;
                    _ea.GetEvent<UpdateChord>().Publish(ch);
                    _ea.GetEvent<UpdateNote>().Publish(n);
                    _repo.Update(n);
                }
            }
        }

        public static void Select(Note n)
        {
            if (n != null)
            {
                //TODO: Isn't there a method to accomplish this conditional evaluation? what is this conditional about?

                if (Collaborations.GetStatus(n) == (int)_Enum.Status.AuthorOriginal ||
                    Collaborations.GetStatus(n) == (int)_Enum.Status.ContributorAdded ||
                    Collaborations.GetStatus(n) == (int)_Enum.Status.AuthorAdded)
                {
                    if (EditorState.DoubleClick)
                    {
                        EditorState.DoubleClick = false;
                        var ng = NotegroupManager.ParseChord(ViewModel.Chord, n);
                        foreach (var g in ng.Notes)
                        {
                            _ea.GetEvent<SelectNote>().Publish(g.Id);
                        }
                    }
                }
            }
        }
    }
}
