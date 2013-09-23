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
        private static DataServiceRepository<Repository.DataService.Composition> _r;
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
            if (_r == null)
            {
                _r = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
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
            Chord = GetOrCreate(Measure.Id, Measure.Index * DurationManager.BPM);
            if (Chord != null)
            {
                var note = NoteController.Create(Chord, Measure, Location_X, Location_Y);
                if (note == null) return null;
                Chord.Notes.Add(note);
                Cache.Notes.Add(note);
                SetNotegroupContext();
                ChordNotegroups = NotegroupManager.ParseChord();
                SetNotegroupContext();
                var ng = NotegroupManager.GetNotegroup(note);
                if (ng != null)
                {
                    note.Orientation = ng.Orientation;
                    _ea.GetEvent<FlagNotegroup>().Publish(ng);

                    var notes = GetActiveNotes(Chord.Notes);
                    if (notes.Count == 1)
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
                        _r.Update(Measure);
                    }
                    note.Location_X = Chord.Location_X;
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

        public static _Enum.NotePlacementMode GetNotePlacementMode(out Chord leftChord, out Chord rightChord)
        {
            leftChord = null;
            rightChord = null;
            var clickX = Location_X + Finetune.Measure.ClickNormalizer_X;
            var mode = GetBracketChords(out leftChord, out rightChord, clickX);
            return mode;
        }

        public static _Enum.NotePlacementMode GetBracketChords(out Chord leftChord, out Chord rightChord, int clickX)
        {
            leftChord = null;
            rightChord = null;
            var leftX = Defaults.MinusInfinity;
            var rightX = Defaults.PlusInfinity;
            var notePlacementMode = _Enum.NotePlacementMode.Append;

            if (!ActiveChords.Any()) return notePlacementMode;
            leftChord = ActiveChords[0];
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            for (var i = 0; i < ActiveChords.Count - 1; i++)
            {
                var ch1 = ActiveChords[i];
                var ch2 = ActiveChords[i + 1];

                if (clickX > ch1.Location_X && clickX < ch2.Location_X)
                {
                    leftChord = ch1;
                    rightChord = ch2;
                    notePlacementMode = _Enum.NotePlacementMode.Insert;
                }
                if (clickX > ch1.Location_X && ch1.Location_X > leftX)
                {
                    leftChord = ch1;
                    leftX = ch1.Location_X;
                }
                if (clickX < ch2.Location_X && ch2.Location_X < rightX)
                {
                    rightChord = ch2;
                    rightX = ch2.Location_X;
                }
            }
            return notePlacementMode;
        }

        public static ObservableCollection<Chord> GetActiveChords(DataServiceCollection<Chord> chords)
        {
            return new ObservableCollection<Chord>((
                from a in chords
                where CollaborationManager.IsActive(a)
                select a).OrderBy(p => p.StartTime));
        }

        //public static ObservableCollection<Chord> GetActiveChordsForSelectedCollaborator(DataServiceCollection<Chord> chords)
        //{
        //    if (chords.Count() == 0) new ObservableCollection<Chord>();
        //    return new ObservableCollection<Chord>((
        //        from a in chords
        //        where CollaborationManager.IsActiveForSelectedCollaborator(a)
        //        select a).OrderBy(p => p.StartTime));
        //}

        public static ObservableCollection<Chord> GetActiveChords(Repository.DataService.Measure measure)
        {
            return GetActiveChords(measure.Chords);
        }

        //public static ObservableCollection<Chord> GetActiveChordsForSelectedCollaborator(Repository.DataService.Measure measure)
        //{
        //    return GetActiveChordsForSelectedCollaborator(measure.Chords);
        //}
        public static ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> notes)
        {
            return new ObservableCollection<Note>((
                from a in notes
                where CollaborationManager.IsActive(a)
                select a).OrderBy(p => p.StartTime));
        }

        public static decimal SetDuration(Chord chord)
        {
            var n = GetActiveNotes(chord.Notes);
            var a = (from c in n select c.Duration);
            var e = a as List<decimal> ?? a.ToList();
            return (!e.Any()) ? chord.Duration : e.Min();
        }

        private static int GetChordXCoordinate(_Enum.NotePlacementMode notePlacementType, Chord chord)
        {
            var locationX = 0;
            var proportionalSpace = DurationManager.GetProportionalSpace();
            var spacing = ((Preferences.SpacingMode == _Enum.MeasureSpacingMode.Constant) ? Measure.Spacing : proportionalSpace);
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            
            switch (notePlacementType)
            {
                case _Enum.NotePlacementMode.Insert:
                    if (_chord1 != null && _chord2 != null)
                    {
                        locationX = _chord1.Location_X + spacing;
                        chord.Location_X = locationX;
                        chord.StartTime = _chord2.StartTime;
                        foreach (Chord c in ActiveChords)  //no need to filter measure.chords using GetActiveChords(). 
                        {
                            if (c.Location_X > _chord1.Location_X && chord != c)
                            {
                                c.Location_X += spacing;
                                if (c.StartTime != null) c.StartTime = (double)c.StartTime + (double)chord.Duration;
                                _r.Update(c);
                            }
                            _ea.GetEvent<SynchronizeChord>().Publish(c);
                            _ea.GetEvent<UpdateChord>().Publish(c);
                        }
                    }
                    break;
                case _Enum.NotePlacementMode.Append:
                    var a = (from c in ActiveChords where c.StartTime < Chord.StartTime select c.Location_X);
                    var e = a as List<int> ?? a.ToList();
                    locationX = (!e.Any()) ? Infrastructure.Constants.Measure.Padding : Convert.ToInt32(e.Max()) + spacing;
                    break;
            }
            return locationX;
        }

        public static Chord GetOrCreate(Guid parentId, decimal startTime)
        {
            //if click is inline with an existing chord, return that chord. otherwise create and return a new chord
            Chord chord;
            if (EditorState.Chord != null)
            {
                //existing active chord
                //the chord duration is set to the minimum duration of its notes.
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
            double st = GetChordStarttime(m_st);
            //what if there's an inactive chord (therefore, noit visible) with the same starttime?
            var a = (from b in Cache.Chords where b.StartTime == st && EditorState.ActiveMeasureId == b.Measure_Id select b);
            var e = a as List<Chord> ?? a.ToList();
            if (e.Any())
            {
                //here the chord exists, but it's inactive. return it.
                chord = e.First();
                EditorState.Chord = chord;
                if (EditorState.Duration != null) chord.Duration = (decimal)EditorState.Duration;
            }
            else
            {
                //brand new chord
                chord = _r.Create<Chord>();
                chord.Id = Guid.NewGuid();
                if (EditorState.Duration != null) chord.Duration = (decimal)EditorState.Duration;
                chord.StartTime = st;
                chord.Measure_Id = parentId;
                chord.Audit = GetAudit();
                chord.Status = CollaborationManager.GetBaseStatus();
            }
            return chord;
        }

        public static Chord Clone(Repository.DataService.Measure measure, Chord source, Collaborator collaborator)
        {
            Chord obj;
            Measure = measure;
            EditorState.Duration = (double)source.Duration;
            obj = GetOrCreate(measure.Id, (decimal)source.StartTime);
            obj.Id = Guid.NewGuid();
            obj.Measure_Id = measure.Id;
            obj.Duration = source.Duration;
            obj.Key_Id = source.Key_Id;
            obj.Location_X = source.Location_X;
            obj.Location_Y = source.Location_Y;
            obj.Audit = GetAudit();
            obj.StartTime = source.StartTime;
            obj.Status = CollaborationManager.GetBaseStatus();
            var index = 0;
            foreach (Note note in source.Notes)
            {
                Note n = NoteController.Clone(obj.Id, source, Measure, obj.Location_X + (index * 16), note.Location_Y, note, collaborator);
                obj.Notes.Add(n);
                index++;
            }
            Cache.Chords.Add(obj);
            InertChords.Add(obj.Id);
            return obj;
        }

        public static Chord Clone(Repository.DataService.Measure m, Chord source)
        {
            Chord obj = null;
            try
            {
                Measure = m;
                EditorState.Duration = (double)source.Duration;
                if (source.StartTime != null)
                {
                    obj = GetOrCreate(m.Id, (decimal)source.StartTime);
                    obj.Id = Guid.NewGuid();
                    obj.Measure_Id = m.Id;
                    obj.Duration = source.Duration;
                    obj.Key_Id = source.Key_Id;
                    obj.Location_X = source.Location_X;
                    obj.Location_Y = source.Location_Y;
                    obj.Audit = GetAudit();
                    obj.StartTime = source.StartTime;
                    obj.Status = CollaborationManager.GetBaseStatus();
                    int index = 0;
                    foreach (var note in source.Notes)
                    {
                        var clonedNote = NoteController.Clone(obj.Id, source, Measure, obj.Location_X + (index * 16),
                                                              note.Location_Y, note);
                        obj.Notes.Add(clonedNote);
                        index++;
                    }
                    Cache.Chords.Add(obj);
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return obj;
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

        public static void Delete(Chord chord)
        {
            //the only way a chord can be deleted is by deleting all of it's notes first. so, every time a note is deleted, this method
            //is called to check and see if the underlying parent chord should be deleted. if so, it is pseudo-deleted by adding a rest to the chord.
            Repository.DataService.Measure measure = (from a in Cache.Measures where a.Id == ViewModel.Chord.Measure_Id select a).First();
            Note rest;
            if (!EditorState.IsCollaboration)
            {
                //if we are deleting the last note (or the only rest) in the chord, and the composition is not under collaboration
                //then delete the chord from the DB and insert a rest in it's place.
                if (ViewModel.Chord.Notes.Count == 0)
                {
                    //add a rest to the empty chord
                    EditorState.Duration = (double)chord.Duration;
                    EditorState.SetRestContext();
                    rest = NoteController.Create(chord, measure, chord.Location_X);
                    rest = NoteController.Deactivate(rest);
                    rest.Pitch = Defaults.RestSymbol;
                    rest.Location_X = chord.Location_X;
                    Cache.Notes.Add(rest);
                    chord.Notes.Add(rest);
                    _r.Update(chord);
                }
            }
            else
            {
                //if isCollaboration, and all notes in the chord are inactive, then start the
                //flow that replaces the chord with a rest.
                if (!CollaborationManager.IsActive(chord))
                {
                    EditorState.Duration = (double)chord.Duration;
                    EditorState.SetRestContext();
                    rest = NoteController.Create(chord, measure, chord.Location_X);
                    rest = NoteController.Deactivate(rest);
                    rest.Pitch = Defaults.RestSymbol;
                    rest.Location_X = chord.Location_X;

                    //the note is already deleted marked as purged. we just need to determine the appropriate status for the rest.
                    //if the deleted note was purgeable (see NoteController) then it was deleted from the DB and the rest status
                    //is set as if it was a normal add to the measure.
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
                        //replaced by a rest but we can't delete the note because the other collaborator may not want to accept 
                        //the delete. so there is a rest and a chord occupying the same starttime. if the collaborator accepts 
                        //the delete, the note can be purged and the rest has its status set appropriately. if the delete is 
                        //rejected, both remain at the same starttime and the rest has its staus set appropriately (see NoteViewModel.OnRejectChange)

                        rest.Status = (EditorState.EditContext == (int)_Enum.EditContext.Authoring) ?
                            Collaborations.SetStatus(rest, (short)_Enum.Status.WaitingOnContributor, 0) :
                            Collaborations.SetStatus(rest, (short)_Enum.Status.WaitingOnAuthor, Collaborations.Index); //replaced a hard coded '0' with 'Collaborations.Index' on 9/27/2012
                    }
                    Cache.Notes.Add(rest);
                    chord.Notes.Add(rest);
                    _r.Update(chord);
                }
            }
            _ea.GetEvent<DeleteTrailingRests>().Publish(measure.Id);
            var chords = GetActiveChords(measure.Chords);
            if (chords.Count > 0)
            {
                _ea.GetEvent<UpdateSpanManager>().Publish(measure.Id);
                MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
                _ea.GetEvent<SpanMeasure>().Publish(measure);
            }
        }

        private static double GetChordStarttime(double m_st)
        {
            var d = Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
            return d + m_st;
        }

        public static void OnSynchronize(Chord ch)
        {
            //when the st or location of a chord changes, then it's constituent notes must be synchronized with the chord. 
            ObservableCollection<Note> notes = GetActiveNotes(ch.Notes);
            foreach (var n in notes)
            {
                if (n.StartTime != ch.StartTime || n.Location_X != ch.Location_X)
                {
                    n.StartTime = ch.StartTime;
                    n.Location_X = ch.Location_X;
                    _ea.GetEvent<UpdateChord>().Publish(ch);
                    _ea.GetEvent<UpdateNote>().Publish(n);
                    _r.Update(n);
                }
            }
        }

        public static void Select(Note note)
        {
            if (note != null)
            {
                //TODO: Isn't there a method to accomplish this conditional evaluation? what is this conditional about?

                if (Collaborations.GetStatus(note) == (int)_Enum.Status.AuthorOriginal ||
                    Collaborations.GetStatus(note) == (int)_Enum.Status.ContributorAdded ||
                    Collaborations.GetStatus(note) == (int)_Enum.Status.AuthorAdded)
                {
                    if (EditorState.DoubleClick)
                    {
                        EditorState.DoubleClick = false;
                        var notegroup = NotegroupManager.ParseChord(ViewModel.Chord, note);
                        foreach (var n in notegroup.Notes)
                        {
                            _ea.GetEvent<SelectNote>().Publish(n.Id);
                        }
                    }
                }
            }
        }
    }
}
