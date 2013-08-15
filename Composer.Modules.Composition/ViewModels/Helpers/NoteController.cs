using System;
using System.Globalization;
using System.Linq;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Dimensions;
using System.Collections.Generic;
using Measure = Composer.Infrastructure.Constants.Measure;

namespace Composer.Modules.Composition.ViewModels
{
    public static class NoteController
    {
        private static readonly DataServiceRepository<Repository.DataService.Composition> Repository;
        public static NoteViewModel ViewModel { get; set; }
        private static readonly IEventAggregator Ea;
        public static Guid SelectedNoteId = Guid.Empty;

        public static int Location_X;
        public static int adjustedY;

        static NoteController()
        {
            Repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            ViewModel = null;
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {
            Ea.GetEvent<DeleteNote>().Subscribe(OnDeleteNote);
        }

        private static bool IsPurgeable(Note note)
        {
            //a note that is purgeable (pending for all collaborations) can be deleted from db instead of setting its status to purged
            bool result = true;
            //if the author of this composition is logged in, and is the note owner...
            if (EditorState.EditContext == _Enum.EditContext.Authoring &&
                note.Audit.CollaboratorIndex == 0)
            {
                string status = note.Status;
                string[] arr = status.Split(',');
                //if none of the collaborators have acted on this note, then the status will be AuthorAdded for all of them.
                //otherwise it's not purgeable.
                for (var i = 1; i <= arr.Length - 1; i++)
                {
                    if (arr[i] != ((int)_Enum.Status.AuthorAdded).ToString(CultureInfo.InvariantCulture))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                //NOT if a contributor to this composition is logged in, and is the note owner, and the author has taken no currentAction...
                if (!(EditorState.EditContext == _Enum.EditContext.Contributing &&
                      note.Audit.CollaboratorIndex == Collaborations.Index &&
                      note.Status == string.Format("{0},{1}", (int)_Enum.Status.PendingAuthorAction, (int)_Enum.Status.ContributorAdded)))
                {
                    result = false;
                }
            }
            //set EditorState.IsPurgeable so that, if true, later in this same note Deletion flow, 
            //the deleted note can be purged, instead of retained with a purged status.
            EditorState.Purgable = result;
            return result;
        }

        public static void OnDeleteNotes(Guid id)
        {

        }

        private static void DeleteRest(Note note)
        {
            var parentChord = (from a in Cache.Chords where a.Id == note.Chord_Id select a).First();
            var parentMeasure = (from a in Cache.Measures where a.Id == parentChord.Measure_Id select a).First();
            Ea.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(parentMeasure.Id, note.Id));
            Ea.GetEvent<MeasureLoaded>().Publish(parentMeasure.Id);
        }

        public static void OnDeleteNote(Note n)
        {
            Chord chord = (from a in Cache.Chords where a.Id == n.Chord_Id select a).First();
            //notes that are purgeable are author notes added by the author that have not been acted on by any collaborator (and the converse of this).
            //such notes can be truly deleted instead of retained with a purged status.
            var isPurgeable = IsPurgeable(n);
            var isRest = IsRest(n);
            if (isRest)
            {
                DeleteRest(n);
            }
            else
            {
                if (!EditorState.IsCollaboration || isPurgeable)
                {
                    chord.Notes.Remove(n);
                    Repository.Delete(n);
                    Cache.Notes.Remove(n);
                }
                else
                {
                    switch (EditorState.EditContext)
                    {
                        case _Enum.EditContext.Authoring: //the logged on user is the composition author
                            n.Audit.CollaboratorIndex = Defaults.AuthorCollaboratorIndex; //TODO: why is this here?

                            //contributed deletions/additions are accepted or rejected by the composition author, but never deleted. So,
                            //AuthorDeleted can only be applied to notes owned by the author. That means that collaboration index is always 0. 
                            //So, hardcoded constant AuthorCollaboratorIndex is OK here.
                            n.Status = Collaborations.SetStatus(n, (int)_Enum.Status.AuthorDeleted, /* index */ Defaults.AuthorCollaboratorIndex);
                            break;
                        case _Enum.EditContext.Contributing: //the logged on user is  NOT the composition author
                            n.Audit.CollaboratorIndex = (short)Collaborations.Index; //TODO: why is this here?
                            n.Status = Collaborations.SetStatus(n, (int)_Enum.Status.ContributorDeleted);
                            break;
                    }
                    n.Audit.ModifyDate = DateTime.Now;
                    Repository.Update(n);
                    Ea.GetEvent<UpdateNote>().Publish(n); //notify viewModel of change
                }
                Ea.GetEvent<DeleteChord>().Publish(chord);
            }
        }

        public static Boolean IsRest(Note n)
        {
            return n.Orientation == (int)_Enum.Orientation.Rest;
        }

        public static Boolean HasFlag(Note note)
        {
            return note.Duration < 1;
        }

        public static Note Create(Chord chord, Repository.DataService.Measure measure, int x)
        {
            return Create(chord, measure, x, 0);
        }

        private static string GetAccidentalSymbol(string acc, string pitchBase)
        {
            var calculatedAccidental = string.Empty;
            if (EditorState.AccidentalNotes.Contains(pitchBase))
            {
                calculatedAccidental = EditorState.TargetAccidental;
            }
            //if the accidental has been applied manually, then prefer it over the calulated accidental.
            return (acc != "" && acc != calculatedAccidental) ? acc : calculatedAccidental;
        }

        public static Chord GetChordFromNote(Note n)
        {
            try
            {
                var b = (from a in Cache.Chords where a.Id == n.Chord_Id select a);
                var e = b as List<Chord> ?? b.ToList();
                return e.Any() ? e.First() : null;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return null;
        }

        public static Repository.DataService.Measure GetMeasureFromNote(Note n)
        {
            try
            {
                var c = GetChordFromNote(n);
                if (c != null)
                {
                    var b = (from a in Cache.Measures where a.Id == c.Measure_Id select a);
                    var e = b as List<Repository.DataService.Measure> ?? b.ToList();
                    return e.Any() ? e.First() : null;
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return null;
        }

        public static Note Create(Chord chord, Repository.DataService.Measure measure, int x, int _clickY)
        {
            //TODO: x parameter appears to be unused.
            Repository.DataService.Note note = null;
            try
            {
                if (EditorState.IsPasting)
                {
                    _clickY = _clickY + Measure.NoteHeight;
                }
                var _acc = "";
                var clickY = _clickY; // locationY is the raw click Y coordinate
                adjustedY = _clickY; // Location_Y is the adjusted Y coordinate.
                note = Repository.Create<Note>();
                note.Type = (short)(EditorState.IsRest() ? 1 : 0);
                note.Id = Guid.NewGuid();
                note.Status = CollaborationManager.GetBaseStatus();
                note.StartTime = chord.StartTime;
                if (EditorState.Duration != null) note.Duration = (decimal)EditorState.Duration;
                if (EditorState.IsNoteSelected())
                {
                    note.Key_Id = Keys.Key.Id;
                    note.Instrument_Id = measure.Instrument_Id;
                    note.IsDotted = EditorState.Dotted;
                    //TODO: having both adjustedY and clickY is redundant. At one time we needed to preserve the original clickY, but not anymore.
                    if (Slot.Normalize_Y.ContainsKey(clickY)) adjustedY = Slot.Normalize_Y[clickY];
                    
                    var slotInfo = Slot.Slot_Y[adjustedY].Split(',');
                    adjustedY += Finetune.Slot.Correction_Y;
                    var pitchBase = slotInfo[1];
                    if (!string.IsNullOrEmpty(EditorState.Accidental))
                    {
                        _acc = (from a in Accidentals.AccidentalList where a.Caption == EditorState.Accidental select a.Name).First();
                    }
                    note.Accidental_Id = null;
                    var acc = GetAccidentalSymbol(_acc, pitchBase);
                    if (acc.Length > 0) note.Accidental_Id = (from a in Accidentals.AccidentalList where a.Name == acc select a.Id).First();
                    var slot = slotInfo[0];
                    if (string.Format("{0}{1}", pitchBase, acc) == "Cb") slot = (int.Parse(slot) - 4).ToString(CultureInfo.InvariantCulture);
                    else if (string.Format("{0}{1}", pitchBase, acc) == "Bs")
                    {
                        slot = (int.Parse(slot) + 4).ToString(CultureInfo.InvariantCulture);
                    }
                    note.Slot = slot;
                    note.Octave_Id = short.Parse(slot.ToCharArray()[0].ToString(CultureInfo.InvariantCulture));
                    note.Pitch = string.Format("{0}{1}{2}", pitchBase, note.Octave_Id, acc);
                    note.Orientation = (Slot.OrientationMap.ContainsKey(note.Slot)) ?
                        (short)Slot.OrientationMap[note.Slot] : (short)_Enum.Orientation.Up;

                    adjustedY = adjustedY - Measure.NoteHeight;
                    note.Location_Y = adjustedY;
                }
                else
                {
                    note.Key_Id = Keys.Key.Id;
                    note.Instrument_Id = null;
                    note.IsDotted = EditorState.Dotted;
                    note.Orientation = (int)_Enum.Orientation.Rest;
                    note.Pitch = Defaults.RestSymbol;
                    note.Location_Y = Finetune.Measure.RestLocation_Y;
                }
                note.Vector_Id = (short)EditorState.VectorId;
                note.Audit = GetAudit();
                note.Chord_Id = chord.Id;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return note;
        }

        public static Note Clone(Guid parentId, Chord chord, Repository.DataService.Measure measure, int x, int y, Note source, Collaborator collaborator)
        {
            Note obj = null;
            try
            {
                obj = Create(chord, measure, x, y);
                obj.Accidental_Id = source.Accidental_Id;
                obj.Duration = source.Duration;
                obj.Instrument_Id = source.Instrument_Id;
                obj.Chord_Id = parentId;
                obj.IsDotted = source.IsDotted;
                obj.IsSpanned = source.IsSpanned;
                obj.Location_X = source.Location_X;
                obj.Location_Y = source.Location_Y;
                obj.Key_Id = source.Key_Id;
                obj.Orientation = source.Orientation;
                obj.Slot = source.Slot;
                obj.Pitch = source.Pitch;
                obj.Type = source.Type;
                obj.Vector_Id = source.Vector_Id;
                obj.StartTime = source.StartTime;
                obj.Status = source.Status;
                obj.Audit = GetAudit();
                Cache.Notes.Add(obj);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return obj;
        }

        public static Note Clone(Guid parentId, Chord chord, Repository.DataService.Measure measure, int x, int y, Note source)
        {
            Note obj = null;
            try
            {
                obj = Create(chord, measure, x, y);
                obj.Accidental_Id = source.Accidental_Id;
                obj.Duration = source.Duration;
                obj.Instrument_Id = source.Instrument_Id;
                obj.Chord_Id = parentId;
                obj.IsDotted = source.IsDotted;
                obj.IsSpanned = source.IsSpanned;
                obj.Location_X = source.Location_X;
                obj.Location_Y = source.Location_Y;
                obj.Key_Id = source.Key_Id;
                obj.Orientation = source.Orientation;
                obj.Slot = source.Slot;
                obj.Pitch = source.Pitch;
                obj.Type = source.Type;
                obj.Vector_Id = source.Vector_Id;
                obj.StartTime = source.StartTime;
                obj.Status = source.Status;
                obj.Audit = GetAudit();

                Cache.Notes.Add(obj);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return obj;
        }

        public static void AddDispositionChangeItem(Note clonedNote, Note cloneSource, _Enum.Disposition disposition)
        {
            if (Collaborations.DispositionChanges == null)
            {
                Collaborations.DispositionChanges = new List<DispositionChangeItem>();
            }
            Collaborations.DispositionChanges.Add(new DispositionChangeItem(clonedNote.Id, cloneSource.Id, disposition));
        }

        public static void AddToCloneMap(Note clonedNote, Note cloneSource)
        {
            if (Collaborations.DispositionChanges == null)
            {
                Collaborations.DispositionChanges = new List<DispositionChangeItem>();
            }
            Collaborations.DispositionChanges.Add(new DispositionChangeItem(clonedNote.Id, cloneSource.Id));
        }

        private static Audit GetAudit()
        {
            var audit = new Audit
                            {
                                CreateDate = DateTime.Now,
                                ModifyDate = DateTime.Now,
                                Author_Id = Current.User.Id,
                                CollaboratorIndex = (short)Collaborations.Index
                            };
            return audit;
        }

        public static void DispatchTool()
        {
            //when a tool that operates on notes, clicks a note, this function routes the click to the appropriate tool handler.
            //tools that do not operate on notes have their clicks handled elsewhere, as appropriate.
            var bDeSelectAll = false;

            ViewModel.Note.Status = ViewModel.Note.Status ?? "0";
            if (CollaborationManager.IsActive(ViewModel.Note))
            {
                switch (EditorState.Tool)
                {
                    case Tool.Select:
                        if (ViewModel.IsSelected)
                            Ea.GetEvent<DeSelectNote>().Publish(ViewModel.Note.Id);
                        else
                            Ea.GetEvent<SelectNote>().Publish(ViewModel.Note.Id);

                        Ea.GetEvent<ChordClicked>().Publish(ViewModel.Note);
                        break;
                    case Tool.Slur:
                        if (ViewModel.IsSelected)
                            Ea.GetEvent<DeSelectNote>().Publish(ViewModel.Note.Id);
                        else
                            Ea.GetEvent<SelectNote>().Publish(ViewModel.Note.Id);

                        Ea.GetEvent<ChordClicked>().Publish(ViewModel.Note);
                        //at this point Slur and Tie lose their distinctness and become Arc.
                        //minor differences in validating, etc are handled in the ArcViewModel, hence the ArcType enum
                        bDeSelectAll = DispatchArcTool(_Enum.ArcType.Slur);
                        break;
                    case Tool.Tie:
                        if (ViewModel.IsSelected)
                        {
                            ViewModel.HideSelector();
                        }
                        else
                        {
                            ViewModel.ShowSelector();
                        }
                        Ea.GetEvent<ChordClicked>().Publish(ViewModel.Note);
                        //at this point Slur and Tie lose their distinctness and become Arc.
                        //minor differences in validating, etc are handled in the ArcViewModel, hence the ArcType enum
                        bDeSelectAll = DispatchArcTool(_Enum.ArcType.Tie);
                        break;
                    case Tool.Clone:
                        break;
                    case Tool.Erase:
                        Ea.GetEvent<DeleteNote>().Publish(ViewModel.Note);
                        break;
                    case Tool.Stem:
                        ReverseStem();
                        break;
                    case Tool.Span:
                        break;
                    case Tool.Edit:
                        break;
                }
            }
            if (bDeSelectAll)
            {
                Ea.GetEvent<DeSelectAll>().Publish(string.Empty);
            }
        }

        private static bool DispatchArcTool(_Enum.ArcType type)
        {
            bool bDeSelectAll = false;
            if (ViewModel.IsSelected)
            {
                if (Infrastructure.Support.Selection.Notes.Count == 2)
                {
                    if (ArcManager.Validate(type))
                    {
                        var c = (from a in Cache.Chords where a.Id == ViewModel.Note.Chord_Id select a).First();
                        var m = (from a in Cache.Measures where a.Id == c.Measure_Id select a).First();
                        var arc = ArcManager.Create(CompositionManager.Composition.Id, m.Staff_Id, type);
                        Ea.GetEvent<AddArc>().Publish(arc);
                    }
                    else
                    {
                        bDeSelectAll = true;
                    }
                }
            }
            return bDeSelectAll;
        }

        private static void ReverseStem()
        {
            var chord = (from obj in Cache.Chords where obj.Id == ViewModel.Note.Chord_Id select obj).SingleOrDefault();
            var measure = (from obj in Cache.Measures where obj.Id == chord.Measure_Id select obj).SingleOrDefault();
            var notegroup = NotegroupManager.ParseChord(chord, ViewModel.Note);

            if (notegroup == null) return;

            foreach (var note in notegroup.Notes)
            {
                Ea.GetEvent<ReverseNoteStem>().Publish(note);
                Repository.Update(note);
            }
            notegroup.Reverse();
            Ea.GetEvent<FlagNotegroup>().Publish(notegroup);
            Ea.GetEvent<SpanMeasure>().Publish(measure);
        }
    }
}