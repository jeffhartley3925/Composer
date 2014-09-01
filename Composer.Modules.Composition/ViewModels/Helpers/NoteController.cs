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
		private static readonly string AuthorAdded = ((int)_Enum.Status.AuthorAdded).ToString(CultureInfo.InvariantCulture);
		private static readonly string ContributorRejectedAdd = ((int)_Enum.Status.ContributorRejectedAdd).ToString(CultureInfo.InvariantCulture);
		private static readonly string AuthorRejectedAdd = ((int)_Enum.Status.AuthorRejectedAdd).ToString(CultureInfo.InvariantCulture);
		private static readonly string PendingAuthorAction = ((int)_Enum.Status.PendingAuthorAction).ToString(CultureInfo.InvariantCulture);

		private static readonly DataServiceRepository<Repository.DataService.Composition> _repository;
		public static NoteViewModel ViewModel { get; set; }
		private static readonly IEventAggregator Ea;
		public static Guid SelectedNoteId = Guid.Empty;

		public static int LocationX;

		static NoteController()
		{
			_repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
			Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
			ViewModel = null;
			SubscribeEvents();
		}

		private static void SubscribeEvents()
		{
			Ea.GetEvent<DeleteNote>().Subscribe(OnDeleteNote);
		}

		private static bool IsPurgeable(Note nT)
		{
			return !CollaborationManager.IsActiveForAnyContributors(nT) && !CollaborationManager.IsActiveForAuthor(nT, 0);
		}

		public static void OnDeleteNotes(Guid id)
		{

		}

		private static void DeleteRest(Note nT, Chord cH)
		{
			var mE = Utils.GetMeasure(cH.Measure_Id);
			Ea.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(mE.Id, nT.Id));
			Ea.GetEvent<MeasureLoaded>().Publish(mE.Id);
		}

		public static void OnDeleteNote(Note nT)
		{
			var cH = Utils.GetChord(nT.Chord_Id);
			// notes that are purgeable are author notes added by the author that have not been acted on by any collaborator (and the converse of this).
			// such notes can be truly deleted instead of retained with a purged status.

			EditorState.Purgable = IsPurgeable(nT);
			// set EditorState.Purgeable so that, if true, then later in this same note deletion flow, 
			// the deleted note can be purged, instead of retained with a purged status.

			if (!EditorState.IsCollaboration || EditorState.Purgable)
			{
				cH.Notes.Remove(nT);
				Cache.Notes.Remove(nT);
				_repository.Delete(nT);

			}
			else
			{
				//if we arrive here, then the underlying chord can never be deleted. it can only have its status changed (or not), to reflect a change.
				switch (EditorState.EditContext)
				{
					case _Enum.EditContext.Authoring: // the logged on user is the composition author
						nT.Audit.CollaboratorIndex = Defaults.AuthorCollaboratorIndex;
						nT.Status = Collaborations.SetStatus(nT, (int)_Enum.Status.AuthorDeleted, /* index */ Defaults.AuthorCollaboratorIndex);
						break;
					case _Enum.EditContext.Contributing: // the logged on user is not the composition author
						nT.Audit.CollaboratorIndex = (short)Collaborations.Index;
						nT.Status = Collaborations.SetStatus(nT, (int)_Enum.Status.ContributorDeleted);
						break;
				}
				nT.Audit.ModifyDate = DateTime.Now;
				_repository.Update(nT);
				Ea.GetEvent<UpdateNote>().Publish(nT); // notify the note vm.
			}
			Ea.GetEvent<DeleteChord>().Publish(cH); // notify the chord that one of its notes was deleted.
		}

		public static Boolean IsRest(Note nT)
		{
			return nT.Orientation == (int)_Enum.Orientation.Rest;
		}

		public static Boolean HasFlag(Note nT)
		{
			return nT.Duration < 1;
		}

		public static Note Create(Chord cH, Repository.DataService.Measure mE)
		{
			return Create(cH, mE, 0);
		}

		private static string GetAccidentalSymbol(string aC, string pIbase)
		{
			var calculatedAccidental = string.Empty;
			if (EditorState.AccidentalNotes.Contains(pIbase))
			{
				calculatedAccidental = EditorState.TargetAccidental;
			}
			//if the accidental has been applied manually, then prefer it over the calculated accidental.
			return (aC != "" && aC != calculatedAccidental) ? aC : calculatedAccidental;
		}

		public static Chord GetChordFromNote(Note nT)
		{
			try
			{
				var b = (from a in Cache.Chords where a.Id == nT.Chord_Id select a);
				var e = b as List<Chord> ?? b.ToList();
				return e.Any() ? e.First() : null;
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return null;
		}

		public static Repository.DataService.Measure GetMeasureFromNote(Note nT)
		{
			try
			{
				var c = GetChordFromNote(nT);
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

		public static Note Activate(Note nT)
		{
			if (nT.Type % Defaults.Deactivator == 0)
			{
				nT.Type = (short)(nT.Type / Defaults.Deactivator);
			}
			if (nT.Type % Defaults.Activator != 0)
			{
				nT.Type = (short)(nT.Type * Defaults.Activator);
			}
			return nT;
		}

		public static Note Deactivate(Note nT)
		{
			if (nT.Type % Defaults.Activator == 0)
			{
				nT.Type = (short)(nT.Type / Defaults.Activator);
			}
			if (nT.Type % Defaults.Deactivator != 0)
			{
				nT.Type = (short)(nT.Type * Defaults.Deactivator);
			}
			return nT;
		}

		public static Note Create(Chord cH, Repository.DataService.Measure mE, int y)
		{
			Note nT = null;
			string slot;
			string pitchBase;
			try
			{
				if (EditorState.IsPasting)
					y = y + Measure.NoteHeight;

				nT = SetSimpleProperties(cH, mE);
				if (EditorState.Duration != null) nT.Duration = (decimal)EditorState.Duration;
				if (EditorState.IsNote())
				{
					if (Slot.Normalize_Y.ContainsKey(y)) y = Slot.Normalize_Y[y];
					var slotInfo = Slot.Slot_Y[y].Split(',');
					y += Finetune.Slot.CorrectionY;
					pitchBase = slotInfo[1];
					var aC = SetAccidental(nT, pitchBase);
					slot = CalculateSlot(slotInfo, pitchBase, aC);
					nT.Slot = slot;
					nT.Octave_Id = short.Parse(nT.Slot.ToCharArray()[0].ToString(CultureInfo.InvariantCulture));
					nT.Pitch = string.Format("{0}{1}{2}", pitchBase, nT.Octave_Id, aC);
					nT.Orientation = (Slot.OrientationMap.ContainsKey(nT.Slot)) ?
						(short)Slot.OrientationMap[nT.Slot] : (short)_Enum.Orientation.Up;
					nT.Location_Y = y - Measure.NoteHeight;
				}
			}
			catch (Exception ex)
			{
				Exceptions.HandleException(ex);
			}
			return nT;
		}

		private static string CalculateSlot(string[] slotInfo, string pIbase, string aC)
		{
			var slot = slotInfo[0];
			switch (string.Format("{0}{1}", pIbase, aC))
			{
				case "Cb":
					slot = (int.Parse(slot) - 4).ToString(CultureInfo.InvariantCulture);
					break;
				case "Bs":
					slot = (int.Parse(slot) + 4).ToString(CultureInfo.InvariantCulture);
					break;
			}
			return slot;
		}

		private static Note SetSimpleProperties(Chord cH, Repository.DataService.Measure mE)
		{
			var nT = _repository.Create<Note>();
			nT.Type = (short)(EditorState.IsRest() ? (int)_Enum.EntityFilter.Rest : (int)_Enum.EntityFilter.Note);
			nT = Activate(nT);
			nT.Id = Guid.NewGuid();
			nT.Status = CollaborationManager.GetBaseStatus();
			nT.StartTime = cH.StartTime;
			nT.IsDotted = EditorState.Dotted;
			nT.Key_Id = Keys.Key.Id;
			if (EditorState.IsNote())
			{
				nT.Instrument_Id = mE.Instrument_Id;
			}
			else
			{
				nT.Instrument_Id = null;
				nT.IsDotted = EditorState.Dotted;
				nT.Orientation = (int)_Enum.Orientation.Rest;
				nT.Pitch = Defaults.RestSymbol;
				nT.Location_Y = Finetune.Measure.RestLocationY;
			}
			nT.Vector_Id = (short)EditorState.VectorId;
			nT.Audit = GetAudit();
			nT.Chord_Id = cH.Id;
			return nT;
		}

		private static string SetAccidental(Note nT, string pIbase)
		{
			var acc = string.Empty;
			if (!string.IsNullOrEmpty(EditorState.Accidental))
			{
				acc = (from a in Accidentals.AccidentalList where a.Caption == EditorState.Accidental select a.Name).First();
			}
			nT.Accidental_Id = null;
			acc = GetAccidentalSymbol(acc, pIbase);
			if (acc.Length > 0)
				nT.Accidental_Id = (from a in Accidentals.AccidentalList where a.Name == acc select a.Id).First();
			return acc;
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

		public static Note Clone(Guid parentId, Chord cH, Repository.DataService.Measure mE, int x, int y, Note source)
		{
			Note obj = null;
			try
			{
				obj = Create(cH, mE, y);
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

		public static void AddDispositionChangeItem(Note clonedNote, Note cloneSource, _Enum.Disposition dI)
		{
			if (Collaborations.DispositionChanges == null)
			{
				Collaborations.DispositionChanges = new List<DispositionChangeItem>();
			}
			Collaborations.DispositionChanges.Add(new DispositionChangeItem(clonedNote.Id, cloneSource.Id, dI));
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
			if (!ViewModel.IsSelected) return false;
			if (Infrastructure.Support.Selection.Notes.Count != 2) return false;
			var deSelectAll = false;
			if (ArcManager.Validate(type))
			{
				var c = (from a in Cache.Chords where a.Id == ViewModel.Note.Chord_Id select a).First();
				var m = (from a in Cache.Measures where a.Id == c.Measure_Id select a).First();
				var arc = ArcManager.Create(CompositionManager.Composition.Id, m.Staff_Id, type);
				Ea.GetEvent<AddArc>().Publish(arc);
			}
			else
			{
				deSelectAll = true;
			}
			return deSelectAll;
		}

		private static void ReverseStem()
		{
			var ch = (from obj in Cache.Chords where obj.Id == ViewModel.Note.Chord_Id select obj).SingleOrDefault();
			if (ch == null) return;

			var ng = NotegroupManager.ParseChord(ch, ViewModel.Note);
			if (ng == null) return;

			foreach (var n in ng.Notes)
			{
				Ea.GetEvent<ReverseNoteStem>().Publish(n);
				_repository.Update(n);
			}
			ng.Reverse();
			Ea.GetEvent<FlagNotegroup>().Publish(ng);

			var m = (from obj in Cache.Measures where obj.Id == ch.Measure_Id select obj).SingleOrDefault();
			if (m == null) return;
			Ea.GetEvent<SpanMeasure>().Publish(m.Id);
		}
	}
}