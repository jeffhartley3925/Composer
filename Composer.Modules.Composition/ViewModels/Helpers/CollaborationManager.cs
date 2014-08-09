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
using System.Diagnostics;

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
                        string.Format(",{0}", (int)_Enum.Status.Meaningless); // this object is actionable for only the author and the current col
                    // it's meaningless for everyone else.
                }

            }
            if (Collaborations.COLLABORATION != null)
            {
                Collaborations.COLLABORATION.LastChangeDate = DateTime.Now;
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
                || s == (int)_Enum.Status.Meaningless
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
                || s == (int)_Enum.Status.Meaningless
                || s == (int)_Enum.Status.Purged);
        }

        private static bool IsActiveForAuthor(Note nT, int idx)
        {
            // USAG: CollaborationManager.IsActionable() x 2

            // the output of this function is meaningless unless.
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId (IE: when the author of the n is a contributor.) because
            // this function is only called as part of a boolean expression that also contains the boolean expression
            // n.Audit.AuthorId != CompositionManager.Composition.Audit.AuthorId

            var s = Collaborations.GetStatus(nT, idx);
            return (
                s == (int)_Enum.Status.AuthorAccepted
                || s == (int)_Enum.Status.AuthorOriginal
                || s == (int)_Enum.Status.ContributorDeleted);
        }

        private static bool IsActiveForContributor(Note n, int idx)
        {
            var s = Collaborations.GetStatus(n, idx);
            return (
                s == (int)_Enum.Status.ContributorAccepted
                || s == (int)_Enum.Status.ContributorRejectedDelete
                || s == (int)_Enum.Status.AuthorOriginal
                || s == (int)_Enum.Status.AuthorDeleted);
        }

        public static bool IsActionable(Note n, Collaborator cL, bool overload)
        {
            //return true;
            return n.Audit.Author_Id == cL.AuthorId;
        }

        // is this note actionable based on its status, authorship, composition authorship, 
        // currently logged on user and selected collaborator? this function answers that question.
        public static bool IsActionable(Note nT, Collaborator cL)
        {
            var result = false;

            // usually we are interested in the current collaborator, but sometimes we need
            // to specify a collaborater, by passing in a currentCollaborator. if currentCollaborator is null then 
            // use the usual Collaborations.CurrentCollaborator.

            if (cL == null && Collaborations.CurrentCollaborator != null)
                cL = Collaborations.CurrentCollaborator;

            var noteAuthoredByAuthor = nT.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id;
            var noteIsAuthoredByContributor = (cL != null) && nT.Audit.Author_Id == cL.AuthorId;
            var noteAuthoredByCurrentUser = nT.Audit.Author_Id == Current.User.Id;

            try
            {
                var mE = Utils.GetMeasure(nT);
                var noteAuthorIndex = GetUserCollaboratorIndex(nT.Audit.Author_Id.ToString(CultureInfo.InvariantCulture));
                var currentUserIndex = GetUserCollaboratorIndex(Current.User.Id);

                var noteIsInactiveForAuthor = IsInactiveForAuthor(nT);
                var noteActiveForAuthor = IsActiveForAuthor(nT, noteAuthorIndex);
                var noteInactiveForContributor = IsInactiveForContributor(nT, currentUserIndex);
                var noteActiveForContributor = IsActiveForContributor(nT, currentUserIndex);

	            var isPackedForAuthor = MeasuregroupManager.IsPacked(mE, 0);
	            //isPackedForAuthor = true;
                if (cL != null)
                {
                    var isPackedForContributor = MeasuregroupManager.IsPacked(mE, cL.Index);

                    if (EditorState.IsAuthor)
                    {
                        var isContributorAdded = Collaborations.GetStatus(nT, cL.Index) == (int)_Enum.Status.ContributorAdded;
                        result = (noteAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || !noteAuthoredByAuthor && noteActiveForAuthor && isPackedForContributor
                                 || noteIsAuthoredByContributor && isContributorAdded && isPackedForContributor);
                    }
                    else
                    {
                        var isAuthorAdded = Collaborations.GetStatus(nT, currentUserIndex) == (int)_Enum.Status.AuthorAdded;
                        result = (noteAuthoredByCurrentUser && !noteInactiveForContributor
                                 || noteAuthoredByAuthor && noteActiveForContributor && isPackedForAuthor
                                 || noteAuthoredByAuthor && isAuthorAdded && isPackedForAuthor);
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
                                 || noteAuthoredByAuthor && noteActiveForContributor && isPackedForAuthor);
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

        private static void SetActivationState(Note n, bool result)
        {
            n = (result) ? NoteController.Activate(n) : NoteController.Deactivate(n);
            var t = n.Type;
            if (n.Type != t) Ea.GetEvent<UpdateNote>().Publish(n);
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

        public static bool IsActive(Note n)
        {
            return IsActive(n, GetCurrentAsCollaborator());
        }

        public static bool IsActive(Note n, Collaborator collaborator)
        {
            if (n.Type < 5 || EditorState.IsCalculatingStatistics)
            {
                var isActionable = (EditorState.IsCalculatingStatistics) ? IsActionable(n, collaborator, true) : IsActionable(n, collaborator);
                return isActionable;
            }
            return n.Type % 5 == 0;
        }
    }
}
