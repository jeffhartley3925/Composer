using System;
using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition.ViewModels
{
	using Composer.Modules.Composition.Annotations;

	public sealed partial class NoteViewModel
	{
		private string status;
		public string Status
		{
			get { return this.status; }
			set
			{
				if (this.status != value)
				{
					this.status = value;
					OnPropertyChanged(() => Status);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="waitingSt"></param>
		/// <param name="deletedSt"></param>
		/// <param name="acceptedSt"></param>
		/// <param name="cH"></param>
		private void AcceptDeletion(int waitingSt, int deletedSt, short acceptedSt, [NotNull] Chord cH)
		{
			// either the author is accepting a contributor deletion, or a contributor is accepting a author deletion. either way,
			// the note is gone forever to both contributor and author. Note: Contributor status is set to Purged here, and 
			// Author status is set to Purged at the end of this method.

			Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.Purged);

			// If this was the last n of the ch when it was deleted, then there will be
			// a note that is not visible, but needs to be made visible.

			var r = (from a in Cache.Notes
					 where Collaborations.GetStatus(a) == waitingSt && // only rests can have a status of 'Limbo'
					 a.StartTime == Note.StartTime
					 select a);

			var e = r as List<Note> ?? r.ToList();
			if (e.Any())
			{
				var rE = e.SingleOrDefault();
				if (rE != null)
				{
					// yes, there is a note, but that doesn't mean we can show the note.
					// first check if there are other deleted notes pending accept/reject in this chord?

					var nT = (from a in Cache.Notes where Collaborations.GetStatus(a) == deletedSt && a.StartTime == Note.StartTime select a);
					NoteController.ResetActivation(rE);
					if (!nT.Any()) //this seems to be a double check. each side of the boolean expression implies the other.
					{
						rE.Status = Collaborations.SetStatus(rE, acceptedSt);
						rE.Status = Collaborations.SetAuthorStatus(rE, (int)_Enum.Status.AuthorOriginal);
					}
				}
			}
			Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
		}

		public void OnRejectChange(Guid iD)
		{
			if (Note.Id == iD)
			{
				var sS = Collaborations.GetStatus(Note);
				switch (EditorState.EditContext)
				{
					case _Enum.EditContext.Authoring:
						switch (sS)
						{
							case (int)_Enum.Status.ContributorAdded:
								Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorRejectedAdd);
								Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorRejectedAdd);
								break;

							case (int)_Enum.Status.ContributorDeleted:
								Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorRejectedDelete);
								Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorRejectedDelete);
								break;
						}
						break;
					case _Enum.EditContext.Contributing:

						switch (sS)
						{
							case (int)_Enum.Status.AuthorAdded:
								Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorRejectedAdd);
								break;

							case (int)_Enum.Status.AuthorDeleted:
								Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorRejectedDelete);
								break;
						}
						break;
				}
				this.Update();
			}
		}

		public void OnAcceptChange(Guid iD)
		{
			if (Note.Id == iD)
			{
				int? sS = Collaborations.GetStatus(Note);
				NoteController.ResetActivation(Note);
				switch (EditorState.EditContext)
				{
					case _Enum.EditContext.Authoring:
						switch (sS)
						{
							case (int)_Enum.Status.ContributorAdded:
								Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorAccepted);
								SetNotegroupContext();
								var nG = NotegroupManager.GetNotegroup(Note);
								EA.GetEvent<FlagNotegroup>().Publish(nG);
								break;

							case (int)_Enum.Status.ContributorDeleted:
								AcceptDeletion((int)_Enum.Status.WaitingOnAuthor, (int)_Enum.Status.ContributorDeleted, (short)_Enum.Status.AuthorAccepted, ParentChord);
								Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
								break;
						}
						break;
					case _Enum.EditContext.Contributing:

						switch (sS)
						{
							case (int)_Enum.Status.AuthorAdded:
								Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorAccepted);
								SetNotegroupContext();
								Notegroup nG = NotegroupManager.GetNotegroup(Note);
								EA.GetEvent<FlagNotegroup>().Publish(nG);
								break;

							case (int)_Enum.Status.AuthorDeleted:
								AcceptDeletion((int)_Enum.Status.WaitingOnContributor, (int)_Enum.Status.AuthorDeleted, (short)_Enum.Status.ContributorAccepted, ParentChord);
								break;
						}
						break;
				}
				this.Update();
			}
		}

		private void Update()
		{
			NoteController.Deactivate(this.Note);
			this.EA.GetEvent<UpdateNote>().Publish(this.Note);
			this.EA.GetEvent<UpdateSpanManager>().Publish(this.ParentMeasure.Id);
			this.EA.GetEvent<SpanMeasure>().Publish(this.ParentMeasure.Id);
		}

		public void SubscribeCollaborationEvents()
		{
			EA.GetEvent<ShowDispositionButtons>().Subscribe(OnShowDispositionButtons);
			EA.GetEvent<RejectChange>().Subscribe(OnRejectChange);
			EA.GetEvent<AcceptChange>().Subscribe(OnAcceptChange);
		}

		public void OnShowDispositionButtons(Tuple<Guid, string> payload)
		{
			var nTiD = payload.Item1;
			if (nTiD != Guid.Empty) return;
			EA.GetEvent<ShowDispositionButtons>().Publish(new Tuple<Guid, string>(Note.Id, Note.Status));
		}

	}
}