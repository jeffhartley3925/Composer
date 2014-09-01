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
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        public static IEventAggregator Ea;
        public static void Initialize()
        {
            if (_repository != null) return;
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            SubscribeEvents();
        }

        private static void SubscribeEvents()
        {

        }

        public static Repository.DataService.Collaboration Create(Repository.DataService.Composition c, int idx)
        {
            Initialize();
            var obj = _repository.Create<Repository.DataService.Collaboration>();
            obj.Id = Guid.NewGuid();
            obj.Composition_Id = c.Id;
            obj.Author_Id = c.Audit.Author_Id;
            obj.Index = idx;
            obj.Collaborator_Id = Current.User.Id;
            obj.Name = Current.User.Name;
            obj.Notes = string.Empty;
            obj.PictureUrl = Current.User.PictureUrl;
            obj.LastChangeDate = DateTime.Now;
            return obj;
        }

        public static bool IsReturningContributor(Repository.DataService.Composition c)
        {
            var a = (from b in c.Collaborations where c.Id == b.Composition_Id && Current.User.Id == b.Collaborator_Id select b);
            return a.Any();
        }

        public static string GetBaseStatus()
        {
            // this method is called from each of the [Entity].Create helper methods.
            var s = string.Empty;
            if (EditorState.IsAuthor)
            {
                s += string.Format("{0}", (int)_Enum.Status.AuthorOriginal);
                if (EditorState.IsCollaboration)
                {
                    if (Collaborations.Collaborators.Count() > 1) // TODO. I'm (almost) certain this 'if' block is redundant. previous 'if' block should suffice.
                    {
                        // for each col Add a status to the status list. 
                        // if there is the author and 3 collaborators. the status for the object is '0,1,1,1' for 'AuthorOriginal, AuthurAdded, AuthurAdded, AuthurAdded'
                        for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                        {
                            s += string.Format(",{0}", (int)_Enum.Status.AuthorAdded);
                        }
                    }
                }
            }
            else
            {
                // for each col, add a default status to the status list
                // the author is notified a col has added a object
                s += string.Format("{0}", (int)_Enum.Status.PendingAuthorAction);
                for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                {
                    // if in addition to the author, there are 3 collaborators, and the current col idx = 2 then
                    // the status for the object is '5,4,8,4', for 'PendingAuthorAction, Meaningless, ContributorAdded, Meaningless'
                    s += (i == Collaborations.Index) ?
                        string.Format(",{0}", (int)_Enum.Status.ContributorAdded) :
                        string.Format(",{0}", (int)_Enum.Status.Null); // this object is actionable for only the author and the current col
                    // it's meaningless for everyone else.
                }

            }
            if (Collaborations.Collaboration != null)
            {
                Collaborations.Collaboration.LastChangeDate = DateTime.Now;
            }
            return s;
        }

        public static bool IsAuthorStatusActive(int s)
        {
            return (
                   s == (int)_Enum.Status.AuthorOriginal
                || s == (int)_Enum.Status.WaitingOnContributor
                || s == (int)_Enum.Status.AuthorAdded
                || s == (int)_Enum.Status.AuthorRejectedDelete
                || s == (int)_Enum.Status.AuthorAccepted
                || s == (int)_Enum.Status.ContributorRejectedAdd);
        }

        private static bool IsInactiveForContributor(Note n, int idx)
        {
            var s = Collaborations.GetStatus(n, idx);
            return (
                s == (int)_Enum.Status.ContributorDeleted
                || s == (int)_Enum.Status.Null
                || s == (int)_Enum.Status.Purged);
        }

        private static bool IsInactiveForAuthor(Note n)
        {

            // the output of this function is meaningless unless.
            // n.Audit.AuthorId == CompositionManager.Composition.Audit.AuthorId (IE: when the author of the n = composition author.) because
            // this function is only called as part of a boolean expression that also contains the boolean expression
            // n.Audit.AuthorId == CompositionManager.Composition.Audit.AuthorId

            var s = Collaborations.GetStatus(n);
            return (
                s == (int)_Enum.Status.AuthorDeleted
                || s == (int)_Enum.Status.Null
                || s == (int)_Enum.Status.Purged);
        }

        public static bool IsActiveForAuthor(Note nT, int iX)
        {
            // USAG: CollaborationManager.IsActionable() x 2

            // the output of this function is meaningless unless.
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId (IE: when the author of the n is a contributor.) because
            // this function is only called as part of a boolean expression that also contains the boolean expression
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId

            var sS = Collaborations.GetStatus(nT, iX);
            return (
                sS == (int)_Enum.Status.AuthorAccepted
                || sS == (int)_Enum.Status.AuthorOriginal
                || sS == (int)_Enum.Status.ContributorDeleted);
        }

        public static bool IsActiveForContributor(Note nT, int iX)
        {
            var sS = Collaborations.GetStatus(nT, iX);
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

		public static bool IsActionable(Note n, Collaborator collaborator, bool unused)
		{
			return true;
		}

        // is this note actionable based on its status, authorship, composition authorship, 
        // currently logged on user and selected collaborator? this function answers that question.
        public static bool IsActionable(Note n, Collaborator collaborator)
        {
            var result = false;

            // usually we are interested in the current collaborator, but sometimes we need
            // to specify a collaborater, by passing in a currentCollaborator. if currentCollaborator is null then 
            // use the usual Collaborations.CurrentCollaborator.

            if (collaborator == null && Collaborations.CurrentCollaborator != null)
                collaborator = Collaborations.CurrentCollaborator;

            var noteAuthoredByAuthor = n.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id;
            var noteIsAuthoredByContributor = (collaborator != null) && n.Audit.Author_Id == collaborator.AuthorId;
            var noteAuthoredByCurrentUser = n.Audit.Author_Id == Current.User.Id;

            try
            {
                var m = Utils.GetMeasure(n);
                var noteAuthorIndex = GetUserCollaboratorIndex(n.Audit.Author_Id.ToString(CultureInfo.InvariantCulture));
                var currentUserIndex = GetUserCollaboratorIndex(Current.User.Id);

                var noteIsInactiveForAuthor = IsInactiveForAuthor(n);
                var noteActiveForAuthor = IsActiveForAuthor(n, noteAuthorIndex);
                var noteInactiveForContributor = IsInactiveForContributor(n, currentUserIndex);
                var noteActiveForContributor = IsActiveForContributor(n, currentUserIndex);

                if (collaborator != null)
                {
                    if (EditorState.IsAuthor)
                    {
                        var isContributorAdded = Collaborations.GetStatus(n, collaborator.Index) == (int)_Enum.Status.ContributorAdded;
                        result = (noteAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || !noteAuthoredByAuthor && noteActiveForAuthor 
                                 || noteIsAuthoredByContributor && isContributorAdded );
                    }
                    else
                    {
                        var isAuthorAdded = Collaborations.GetStatus(n, currentUserIndex) == (int)_Enum.Status.AuthorAdded;
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
            SetActivationState(n, result);
            return result;
        }

        public static void SetActivationState(Note n, bool result)
        {
			var type = n.Type;
            n = (result) ? NoteController.Activate(n) : NoteController.Deactivate(n);
            
            if (n.Type != type) Ea.GetEvent<UpdateNote>().Publish(n);

        }

        private static int GetUserCollaboratorIndex(string id)
        {
            var index = 0;
            var q = (from a in Collaborations.CurrentCollaborations where a.CollaboratorId == id select a.Index);
            var e = q as List<int> ?? q.ToList();
            if (e.Any())
                index = e.First();
            return index;
        }

        public static Collaborator GetCurrentAsCollaborator()
        {
            return Collaborations.CurrentCollaborator;
        }

        public static Collaborator GetSpecifiedCollaborator(int index)
        {
            var q = (from a in Collaborations.Collaborators where a.Index == index select a);
            var e = q as List<Collaborator> ?? q.ToList();
            return e.Any() ? e.First() : null;
        }

        public static bool IsPendingAdd(int? s)
        {
            if (s == null) return false;
            return (s == (int)_Enum.Status.AuthorAdded && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.ContributorAdded && EditorState.EditContext == _Enum.EditContext.Authoring
                    || s == (int)_Enum.Status.PendingAuthorAction && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsPendingDelete(int? s)
        {
            if (s == null) return false;
            return (s == (int)_Enum.Status.AuthorDeleted && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.ContributorDeleted && EditorState.EditContext == _Enum.EditContext.Authoring
                    || s == (int)_Enum.Status.WaitingOnContributor && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.WaitingOnAuthor && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsActive(Chord ch)
        {
            return IsActive(ch, GetCurrentAsCollaborator());
        }

        public static bool IsActive(Chord ch, Collaborator collaborator)
        {
            var result = false;
            try
            {
                var a = (from n in ch.Notes
                         where (IsActive(n, collaborator))
                         select n);
                result = a.Any();
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActive(Repository.DataService.Chord ch, bool isLoading)");
            }
            return result;
        }

        public static bool IsActiveForSelectedCollaborator(Chord ch, Collaborator col)
        {
            var result = false;
            try
            {
                var a = (from n in ch.Notes
                         where (IsActionable(n, col))
                         select n);
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

        public static bool IsActive(Note nT, Collaborator collaborator)
        {
            if (nT.Type < 5 || EditorState.IsCalculatingStatistics)
            {
				var isActionable = (EditorState.IsCalculatingStatistics) ? IsActionable(nT, collaborator, true) : IsActionable(nT, collaborator);
                return isActionable;
            }
            return nT.Type % 5 == 0;
        }
    }
}
