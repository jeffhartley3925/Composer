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
        private static readonly string AuthorAdded = ((int)_Enum.Status.AuthorAdded).ToString();
        private static readonly string ContributorRejectedAdd = ((int)_Enum.Status.ContributorRejectedAdd).ToString();
        private static readonly string AuthorRejectedAdd = ((int)_Enum.Status.AuthorRejectedAdd).ToString();
        private static readonly string PendingAuthorAction = ((int)_Enum.Status.PendingAuthorAction).ToString();

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

        private static bool IsPurgeable(Note n)
        {
            //this method is called when deleting a object.
            //a note that is purgeable can be deleted from the db instead of maintaining it with a status of Purged
            bool result = true;
            //if the author of this composition is logged in, and is the note owner...
            string cds = n.Status;
            string[] a = cds.Split(',');
            if (EditorState.EditContext == _Enum.EditContext.Authoring && n.Audit.CollaboratorIndex == 0)
            {
                //if this note was AuthorAdded after collaborations began, then the note can be 
                //Purged as long as no contributor has acted to keep the AuthorAdded.

                //if none of the collaborators have acted on the AuthorAdded, or have acted, but with a ContributorRejectedAdd
                //then the object can be Purged.

                for (var i = 1; i <= a.Length - 1; i++)
                {
                    if (a[i] != AuthorAdded && a[i] != ContributorRejectedAdd)
                    {
                        //ok, one or more contributors decided to keep the object, so we cannot Purge it.
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                //NOT if a contributor to this composition is logged in, and is the note owner, and the author has taken no action, or
                //the author did take action by AuthorRejectedAdd 
                if (EditorState.EditContext == _Enum.EditContext.Contributing && n.Audit.CollaboratorIndex == Collaborations.Index)
                {
                    if (a[0] != PendingAuthorAction && a[0] != AuthorRejectedAdd)
                    {
                        //ok, the author decided to keep the object, so we cannot Purge it.
                        result = false;
                    }
                }
            }

            //set EditorState.Purgeable so that, if true, later in this same note Deletion flow, 
            //the deleted note can be purged, instead of retained with a purged status.
            EditorState.Purgable = result;
            return result;
        }

        public static void OnDeleteNotes(Guid id)
        {

        }

        private static void DeleteRest(Note n)
        {
            var nChord = (from a in Cache.Chords where a.Id == n.Chord_Id select a).First();
            var nMeasure = (from a in Cache.Measures where a.Id == nChord.Measure_Id select a).First();
            Ea.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(nMeasure.Id, n.Id));
            Ea.GetEvent<MeasureLoaded>().Publish(nMeasure.Id);
        }

        public static void OnDeleteNote(Note n)
        {
            Chord chord = (from a in Cache.Chords where a.Id == n.Chord_Id select a).First();
            //notes that are purge-able are author notes added by the author that have not been acted on by any collaborator (and the converse of this).
            //such notes can be truly deleted instead of retained with a purged status.

            //TODO: not sure why deleting a rest is somehow different than deleting a note;
            var isRest = IsRest(n);
            if (isRest)
            {
                DeleteRest(n);
            }
            else
            {
                if (!EditorState.IsCollaboration || EditorState.Purgable)
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
                            n.Status = Collaborations.SetStatus(n, (int)_Enum.Status.AuthorDeleted, /* index */ Defaults.AuthorCollaboratorIndex);
                            break;
                        case _Enum.EditContext.Contributing: //the logged on user is NOT the composition author
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

        public static Note Create(Chord ch, Repository.DataService.Measure m)
        {
            return Create(ch, m, 0);
        }

        private static string GetAccidentalSymbol(string acc, string pitchBase)
        {
            var calculatedAccidental = string.Empty;
            if (EditorState.AccidentalNotes.Contains(pitchBase))
            {
                calculatedAccidental = EditorState.TargetAccidental;
            }
            //if the accidental has been applied manually, then prefer it over the calculated accidental.
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

        public static Repository.DataService.Note Activate(Repository.DataService.Note n)
        {
            if (n.Type % Defaults.Deactivator == 0)
            {
                n.Type = (short)(n.Type / Defaults.Deactivator);
            }
            if (n.Type % Defaults.Activator != 0)
            {
                n.Type = (short)(n.Type * Defaults.Activator);
            }
            return n;
        }

        public static Repository.DataService.Note Deactivate(Repository.DataService.Note n)
        {
            if (n.Type % Defaults.Activator == 0)
            {
                n.Type = (short)(n.Type / Defaults.Activator);
            }
            if (n.Type % Defaults.Deactivator != 0)
            {
                n.Type = (short)(n.Type * Defaults.Deactivator);
            }
            return n;
        }

        public static Note Create(Chord ch, Repository.DataService.Measure m, int clickY)
        {
            //TODO: x parameter appears to be unused.
            Note note = null;
            try
            {
                if (EditorState.IsPasting)
                {
                    clickY = clickY + Measure.NoteHeight;
                }

                adjustedY = clickY; // Location_Y is the adjusted Y coordinate.
                note = Repository.Create<Note>();
                note.Type = (short)(EditorState.IsRest() ? (int)_Enum.ObjectType.Rest : (int)_Enum.ObjectType.Note);
                note = Activate(note);
                note.Id = Guid.NewGuid();
                note.Status = CollaborationManager.GetBaseStatus();
                note.StartTime = ch.StartTime;
                if (EditorState.Duration != null) note.Duration = (decimal)EditorState.Duration;
                if (EditorState.IsNote())
                {
                    note.Key_Id = Keys.Key.Id;
                    note.Instrument_Id = m.Instrument_Id;
                    note.IsDotted = EditorState.Dotted;
                    if (Slot.Normalize_Y.ContainsKey(clickY)) clickY = Slot.Normalize_Y[clickY];

                    var slotInfo = Slot.Slot_Y[clickY].Split(',');
                    clickY += Finetune.Slot.Correction_Y;
                    var pitchBase = slotInfo[1];
                    var acc = string.Empty;
                    if (!string.IsNullOrEmpty(EditorState.Accidental))
                    {
                        acc = (from a in Accidentals.AccidentalList where a.Caption == EditorState.Accidental select a.Name).First();
                    }
                    note.Accidental_Id = null;
                    acc = GetAccidentalSymbol(acc, pitchBase);
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
                note.Chord_Id = ch.Id;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return note;
        }

        public static Note Clone(Guid parentId, Chord chord, Repository.DataService.Measure measure, int x, int y, Note source, Collaborator collaborator)
        {
            Note o = null;
            try
            {
                o = Create(chord, measure, y);
                o.Accidental_Id = source.Accidental_Id;
                o.Duration = source.Duration;
                o.Instrument_Id = source.Instrument_Id;
                o.Chord_Id = parentId;
                o.IsDotted = source.IsDotted;
                o.IsSpanned = source.IsSpanned;
                o.Location_X = source.Location_X;
                o.Location_Y = source.Location_Y;
                o.Key_Id = source.Key_Id;
                o.Orientation = source.Orientation;
                o.Slot = source.Slot;
                o.Pitch = source.Pitch;
                o.Type = source.Type;
                o.Vector_Id = source.Vector_Id;
                o.StartTime = source.StartTime;
                o.Status = source.Status;
                o.Audit = GetAudit();
                Cache.Notes.Add(o);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
            return o;
        }

        public static Note Clone(Guid parentId, Chord chord, Repository.DataService.Measure measure, int x, int y, Note source)
        {
            Note obj = null;
            try
            {
                obj = Create(chord, measure, y);
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