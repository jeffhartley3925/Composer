using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels.Helpers;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public static class CollaborationManager
    {
        private static DataServiceRepository<Repository.DataService.Composition> repository;
        public static IEventAggregator Ea;
        public static void Initialize()
        {
            if (repository != null) return;
            repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {

        }

        public static Repository.DataService.Collaboration Create(Repository.DataService.Composition cO, int iX)
        {
            Initialize();
            var obj = repository.Create<Repository.DataService.Collaboration>();
            obj.Id = Guid.NewGuid();
            obj.Composition_Id = cO.Id;
            obj.Author_Id = cO.Audit.Author_Id;
            obj.Index = iX;
            obj.Collaborator_Id = Current.User.Id;
            obj.Name = Current.User.Name;
            obj.Notes = string.Empty;
            obj.PictureUrl = Current.User.PictureUrl;
            obj.LastChangeDate = DateTime.Now;
            return obj;
        }

        public static bool IsReturningContributor(Repository.DataService.Composition cO)
        {
            return (from b in cO.Collaborations 
					where cO.Id == b.Composition_Id && Current.User.Id == b.Collaborator_Id 
					select b).Any();
        }

        public static string GetBaseStatus()
        {
            // this method is called from each of the [Entity].Create helper methods.
            var sS = string.Empty;
            if (EditorState.IsAuthor)
            {
                sS += string.Format("{0}", (int)_Enum.Status.AuthorOriginal);
                if (EditorState.IsCollaboration)
                {
                    if (Collaborations.Collaborators.Count() > 1) // TODO. I'm (almost) certain this 'if' block is redundant. previous 'if' block should suffice.
                    {
                        // for each col Add a status to the status list. 
                        // if there is the author and 3 collaborators. the status for the object is '0,1,1,1' for 'AuthorOriginal, AuthurAdded, AuthurAdded, AuthurAdded'
                        for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                        {
                            sS += string.Format(",{0}", (int)_Enum.Status.AuthorAdded);
                        }
                    }
                }
            }
            else
            {
                // for each col, add a default status to the status list
                // the author is notified a col has added a object
                sS += string.Format("{0}", (int)_Enum.Status.PendingAuthorAction);
                for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                {
                    // if in addition to the author, there are 3 collaborators, and the current col idx = 2 then
                    // the status for the object is '5,4,8,4', for 'PendingAuthorAction, Meaningless, ContributorAdded, Meaningless'
                    sS += (i == Collaborations.Index) ?
                        string.Format(",{0}", (int)_Enum.Status.ContributorAdded) :
                        string.Format(",{0}", (int)_Enum.Status.Null); // this object is actionable for only the author and the current col
                    // it's meaningless for everyone else.
                }

            }
            if (Collaborations.Collaboration != null)
            {
                Collaborations.Collaboration.LastChangeDate = DateTime.Now;
            }
            return sS;
        }

        public static bool IsAuthorStatusActive(int sS)
        {
            return (
                   sS == (int)_Enum.Status.AuthorOriginal
                || sS == (int)_Enum.Status.WaitingOnContributor
                || sS == (int)_Enum.Status.AuthorAdded
                || sS == (int)_Enum.Status.AuthorRejectedDelete
                || sS == (int)_Enum.Status.AuthorAccepted
                || sS == (int)_Enum.Status.ContributorRejectedAdd);
        }

        private static bool IsInactiveForContributor(Note nT, int cRiX)
        {
            var sS = Collaborations.GetStatus(nT, cRiX);
            return (
                sS == (int)_Enum.Status.ContributorDeleted
                || sS == (int)_Enum.Status.Null
                || sS == (int)_Enum.Status.Purged);
        }

        private static bool IsInactiveForAuthor(Note nT)
        {

            // the output of this function is meaningless unless.
            // n.Audit.AuthorId == CompositionManager.Composition.Audit.AuthorId (IE: when the author of the n = composition author.) because
            // this function is only called as part of a boolean expression that also contains the boolean expression
            // n.Audit.AuthorId == CompositionManager.Composition.Audit.AuthorId

            var sS = Collaborations.GetStatus(nT);
            return (
                sS == (int)_Enum.Status.AuthorDeleted
                || sS == (int)_Enum.Status.Null
                || sS == (int)_Enum.Status.Purged);
        }

        public static bool IsActiveForAuthor(Note nT, int cRiX)
        {
            // USAG: CollaborationManager.IsActionable() x 2

            // the output of this function is meaningless unless.
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId (IE: when the author of the n is a contributor.) because
            // this function is only called as part of a boolean expression that also contains the boolean expression
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId

            var sS = Collaborations.GetStatus(nT, cRiX);
            return (
                sS == (int)_Enum.Status.AuthorAccepted
                || sS == (int)_Enum.Status.AuthorOriginal
                || sS == (int)_Enum.Status.ContributorDeleted);
        }

        public static bool IsActiveForContributor(Note nT, int cRiX)
        {
            var sS = Collaborations.GetStatus(nT, cRiX);
            return (
                sS == (int)_Enum.Status.ContributorAccepted
                || sS == (int)_Enum.Status.ContributorRejectedDelete
                || sS == (int)_Enum.Status.AuthorOriginal
                || sS == (int)_Enum.Status.AuthorDeleted);
        }

		public static bool IsActiveForAnyContributors(Note nT)
		{
			var sSs = nT.Status.Split(',');
			return sSs.Any(sS => IsActiveForContributor(nT, int.Parse(sS)));
		}

        // is this note actionable based on its status, authorship, composition authorship, 
        // currently logged on user and selected collaborator? this function answers that question.
        public static bool IsActionable(Note nT, Collaborator collaborator)
        {
            var result = false;

            // usually we are interested in the current collaborator, but sometimes we need
            // to specify a collaborater, by passing in a currentCollaborator. if currentCollaborator is null then 
            // use the usual Collaborations.CurrentCollaborator.

            if (collaborator == null && Collaborations.CurrentCollaborator != null)
                collaborator = Collaborations.CurrentCollaborator;

            var noteAuthoredByAuthor = nT.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id;
            var noteIsAuthoredByContributor = (collaborator != null) && nT.Audit.Author_Id == collaborator.AuthorId;
            var noteAuthoredByCurrentUser = nT.Audit.Author_Id == Current.User.Id;

            try
            {
                var m = Utils.GetMeasure(nT);
                var noteAuthorIndex = GetUserCollaboratorIndex(nT.Audit.Author_Id.ToString(CultureInfo.InvariantCulture));
                var currentUserIndex = GetUserCollaboratorIndex(Current.User.Id);

                var noteIsInactiveForAuthor = IsInactiveForAuthor(nT);
                var noteActiveForAuthor = IsActiveForAuthor(nT, noteAuthorIndex);
                var noteInactiveForContributor = IsInactiveForContributor(nT, currentUserIndex);
                var noteActiveForContributor = IsActiveForContributor(nT, currentUserIndex);

                if (collaborator != null)
                {
                    if (EditorState.IsAuthor)
                    {
                        var isContributorAdded = Collaborations.GetStatus(nT, collaborator.Index) == (int)_Enum.Status.ContributorAdded;
                        result = (noteAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || !noteAuthoredByAuthor && noteActiveForAuthor 
                                 || noteIsAuthoredByContributor && isContributorAdded );
                    }
                    else
                    {
                        var isAuthorAdded = Collaborations.GetStatus(nT, currentUserIndex) == (int)_Enum.Status.AuthorAdded;
                        result = (noteAuthoredByCurrentUser && !noteInactiveForContributor
                                 || noteAuthoredByAuthor && noteActiveForContributor 
                                 || noteAuthoredByAuthor && isAuthorAdded );
                    }
                }
                else
                {
                    if (EditorState.IsAuthor)
                    {
                        result = (noteAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || !noteAuthoredByAuthor && noteActiveForAuthor);
                    }
                    else
                    {
                        result = (noteAuthoredByCurrentUser && !noteInactiveForContributor
                                 || noteAuthoredByAuthor && noteActiveForContributor );
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActionable(Repository.DataService.Note n, Collaborator currentCollaborator)");
            }
            SetActivationState(nT, result);
            return result;
        }

        public static void SetActivationState(Note nT, bool result)
        {
			var type = nT.Type;
            nT = (result) ? NoteController.Activate(nT) : NoteController.Deactivate(nT);
            if (nT.Type != type) Ea.GetEvent<UpdateNote>().Publish(nT);
        }

        private static int GetUserCollaboratorIndex(string iD)
        {
            var index = 0;
            var q = (from a in Collaborations.CurrentCollaborations where a.CollaboratorId == iD select a.Index);
            var e = q as List<int> ?? q.ToList();
            if (e.Any())
                index = e.First();
            return index;
        }

        public static Collaborator GetCurrentAsCollaborator()
        {
            return Collaborations.CurrentCollaborator;
        }

        public static Collaborator GetSpecifiedCollaborator(int cRiX)
        {
            var q = (from a in Collaborations.Collaborators where a.Index == cRiX select a);
            var e = q as List<Collaborator> ?? q.ToList();
            return e.Any() ? e.First() : null;
        }

        public static bool IsPendingAdd(int? sS)
        {
            if (sS == null) return false;
            return (sS == (int)_Enum.Status.AuthorAdded && EditorState.EditContext == _Enum.EditContext.Contributing
                    || sS == (int)_Enum.Status.ContributorAdded && EditorState.EditContext == _Enum.EditContext.Authoring
                    || sS == (int)_Enum.Status.PendingAuthorAction && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsPendingDelete(int? sS)
        {
            if (sS == null) return false;
            return (sS == (int)_Enum.Status.AuthorDeleted && EditorState.EditContext == _Enum.EditContext.Contributing
                    || sS == (int)_Enum.Status.ContributorDeleted && EditorState.EditContext == _Enum.EditContext.Authoring
                    || sS == (int)_Enum.Status.WaitingOnContributor && EditorState.EditContext == _Enum.EditContext.Contributing
                    || sS == (int)_Enum.Status.WaitingOnAuthor && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsActive(Chord cH)
        {
            return IsActive(cH, GetCurrentAsCollaborator());
        }

        public static bool IsActive(Chord cH, Collaborator cR)
        {
            var result = false;
            try
            {
                var a = (from nT in cH.Notes
                         where (IsActive(nT, cR))
                         select nT);
                result = a.Any();
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActive(Repository.DataService.Chord ch, bool isLoading)");
            }
            return result;
        }

        public static bool IsActiveForSelectedCollaborator(Chord cH, Collaborator cR)
        {
            var result = false;
            try
            {
                var a = (from nT in cH.Notes
                         where (IsActionable(nT, cR))
                         select nT);
                result = a.Any();
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActive(Repository.DataService._chord ch, bool isLoading)");
            }
            return result;
        }

        public static bool IsActive(Note nT)
        {
            return IsActive(nT, GetCurrentAsCollaborator());
        }

        public static bool IsActive(Note nT, Collaborator cR)
        {
            if (nT.Type < 5)
            {
				return IsActionable(nT, cR);
            }
            return nT.Type % 5 == 0;
        }
    }
}
