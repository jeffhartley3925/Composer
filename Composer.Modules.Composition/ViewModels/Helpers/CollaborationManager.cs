using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public static class CollaborationManager
    {
        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        public static void Initialize()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                SubscribeEvents();
            }
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

        public static bool IsContributing(Repository.DataService.Composition c)
        {
            var a = (from b in c.Collaborations where c.Id == b.Composition_Id && Current.User.Id == b.Collaborator_Id select b);
            return a.Any();
        }

        public static string GetBaseStatus()
        {
            //this method is called from each of the [Entity].Create helper methods.
            var baseStatus = string.Empty;
            if (EditorState.IsAuthor)
            {
                baseStatus += string.Format("{0}", (int)_Enum.Status.AuthorOriginal);
                if (EditorState.IsCollaboration)
                {
                    if (Collaborations.Collaborators.Count() > 1) //TODO. I'm (almost) certain this 'if' block is redundant. previous 'if' block should suffice.
                    {
                        //for each collaborator Add a status to the status list. 
						//if there is the author and 3 collaborators. the status for the object is '0,1,1,1' for 'AuthorOriginal, AuthurAdded, AuthurAdded, AuthurAdded'
                        for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                        {
                            baseStatus += string.Format(",{0}", (int)_Enum.Status.AuthorAdded);
                        }
                    }
                }
            }
            else
            {
                //for each collaborator, add a default status to the status list
                //the author is notified a collaborator has added a object
                baseStatus += string.Format("{0}", (int)_Enum.Status.PendingAuthorAction);
                for (var i = 1; i <= Collaborations.Collaborators.Count() - 1; i++)
                {
					//if in addition to the author, there are 3 collaborators, and the current collaborator index = 2 then
					//the status for the object is '5,4,8,4', for 'PendingAuthorAction, Meaningless, ContributorAdded, Meaningless'
                    baseStatus += (i == Collaborations.Index) ?
                        string.Format(",{0}", (int)_Enum.Status.ContributorAdded) :
                        string.Format(",{0}", (int)_Enum.Status.Meaningless); //this object is actionable for only the author and the current collaborator
                                                                              //it's meaningless for everyone else.
                }

            }
            if (Collaborations.COLLABORATION != null)
            {
                Collaborations.COLLABORATION.LastChangeDate = DateTime.Now;
            }
            return baseStatus;
        }

        public static bool IsAuthorStatusActive(int s)
        {
            //USED IN: Collaborations.SetAuthorStatus()
            //         Collaborations.SetStatus()
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
            //USED IN: CollaborationManager.IsActionable() x 2
            var s = Collaborations.GetStatus(n, idx);
            return (
                s == (int)_Enum.Status.ContributorDeleted
                || s == (int)_Enum.Status.Meaningless
                || s == (int)_Enum.Status.Purged);
        }

        private static bool IsInactiveForAuthor(Note n)
        {
            //USED IN: CollaborationManager.IsActionable() x 2

            //the ouput of this function is meaningless unless....
            //...note.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id (ie: when the author of the note = composition author.) because
            //this function is only called as part of a boolean expression that also contains the boolean expression...
            //...note.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id

            var s = Collaborations.GetStatus(n);
            return (
                s == (int)_Enum.Status.AuthorDeleted
                || s == (int)_Enum.Status.Meaningless
                || s == (int)_Enum.Status.Purged);
        }

        private static bool IsActiveForAuthor(Note n, int idx)
        {
            //USAG: CollaborationManager.IsActionable() x 2

            //the ouput of this function is meaningless unless....
            //...note.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id (ie: when the author of the note is a contributor.) because
            //this function is only called as part of a boolean expression that also contains the boolean expression...
            //...note.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id

            //this function is only called in boolean expressions containing the expression...
            //...note.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id
            var s = Collaborations.GetStatus(n, idx);
            return (
                s == (int)_Enum.Status.AuthorAccepted
                || s == (int)_Enum.Status.AuthorOriginal //should this be here? status = AuthorOriginal signifies
                                                              //the note author is the composition author.
                || s == (int)_Enum.Status.ContributorDeleted); //should this be here? status = ContributorDeleted signifies the 
                                                                    //note author is the composition author.
        }

        private static bool IsActiveForContributor(Note n, int idx)
        {
            //USED IN: CollaborationManager.IsActionable() x 2
            var s = Collaborations.GetStatus(n, idx);
            return (
                s == (int)_Enum.Status.ContributorAccepted
                || s == (int)_Enum.Status.ContributorRejectedDelete
                || s == (int)_Enum.Status.AuthorOriginal
                || s == (int)_Enum.Status.AuthorDeleted);
        }

        private static bool _checkingPackedState;

        //is this note actionable based on its status, note authorship, composition authorship?
        //and the currently logged on user. this function answers that question.
        public static bool IsActionable(Note n, Collaborator col)
        {
            var result = false;

            //usually we are interested in the current collaborator, but sometimes we need
            //to specify a collaborater, by passing in a currentCollaborator. if currentCollaborator is null then 
            //use Collaborations.CurrentCollaborator.
            if (col == null && Collaborations.CurrentCollaborator != null)
            {
                col = Collaborations.CurrentCollaborator;
            }

            var noteIsAuthoredByAuthor = n.Audit.Author_Id == CompositionManager.Composition.Audit.Author_Id;
            var noteIsAuthoredByContributor = n.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id;
            var noteIsAuthoredBySpecifiedContributor = (col != null) && n.Audit.Author_Id == col.Author_Id;
            var noteIsAuthoredByCurrentUser = n.Audit.Author_Id == Current.User.Id;

            try
            {
                int idx;
                bool noteIsInactiveForAuthor;
                bool noteIsActiveForAuthor;
                bool noteIsInactiveForContributor;
                bool noteIsActiveForContributor;

                if (col != null)
                {
                    //only allow packed measures to display contributor submissions
                    var isPackedMeasure = true;
                    if (!_checkingPackedState)
                    {
                        //IsPackedMeasure calls this function, so we need a mechanism to avoid circularity.
                        _checkingPackedState = true;
                        isPackedMeasure = MeasureManager.IsPackedMeasure(NoteController.GetMeasureFromNote(n));
                        _checkingPackedState = false;
                        isPackedMeasure = true;
                    }

                    if (EditorState.IsAuthor)
                    {
                        //the currently logged on user is the author, and there is a col selected in the collaboration panel.
                        idx = GetUserCollaboratorIndex(n.Audit.Author_Id.ToString(CultureInfo.InvariantCulture));

                        noteIsInactiveForAuthor = IsInactiveForAuthor(n);
                        noteIsActiveForAuthor = IsActiveForAuthor(n, idx);
                        var isContributorAdded = Collaborations.GetStatus(n, col.Index) == (int)_Enum.Status.ContributorAdded;

                        result = (noteIsAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || noteIsAuthoredByContributor && noteIsActiveForAuthor
                                 || isPackedMeasure && noteIsAuthoredBySpecifiedContributor && isContributorAdded); //ie: note is pending
                    }
                    else
                    {
                        //the currently logged on user is a contributor, and the composition author is selected in the collaboration panel.
                        idx = GetUserCollaboratorIndex(Current.User.Id);

                        noteIsInactiveForContributor = IsInactiveForContributor(n, idx);
                        noteIsActiveForContributor = IsActiveForContributor(n, idx);
                        var isAuthorAdded = Collaborations.GetStatus(n, idx) == (int)_Enum.Status.AuthorAdded;

                        result = (noteIsAuthoredByCurrentUser && !noteIsInactiveForContributor
                                 || noteIsAuthoredByAuthor && noteIsActiveForContributor
                                 || isPackedMeasure && noteIsAuthoredByAuthor && isAuthorAdded);  //ie: note is pending
                    }
                }
                else
                {
                    //is the currently logged in user the composition author?
                    if (EditorState.IsAuthor)
                    {
                        idx = GetUserCollaboratorIndex(n.Audit.Author_Id.ToString(CultureInfo.InvariantCulture));

                        //arriving here means that the currently logged on user is the author of the composition, and there 
                        //isn't a target contributor selected in the collaboration panel

                        //even though the currently logged in user is the composition author, notes authored by the composition 
                        //author may not be active, and notes authored by a contributor may be inactive.

                        noteIsInactiveForAuthor = IsInactiveForAuthor(n);
                        noteIsActiveForAuthor = IsActiveForAuthor(n, idx);

                        result = (noteIsAuthoredByAuthor && !noteIsInactiveForAuthor
                                 || noteIsAuthoredByContributor && noteIsActiveForAuthor);
                    }
                    else
                    {
                        //arriving here means that the currently logged on user is a contributor to the composition, and the 
                        //author isn't selected in the collaboration panel.

                        idx = GetUserCollaboratorIndex(Current.User.Id);

                        //even though the currently logged in user is a contributor, notes authored by the contributor 
                        //may not be active, and notes authored by the composition author may be inactive.

                        noteIsInactiveForContributor = IsInactiveForContributor(n, idx);
                        noteIsActiveForContributor = IsActiveForContributor(n, idx);

                        result = (noteIsAuthoredByCurrentUser && !noteIsInactiveForContributor
                                 || noteIsAuthoredByAuthor && noteIsActiveForContributor);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActionable(Repository.DataService.Note n, Collaborator currentCollaborator)");
            }
            return result;
        }

        private static int GetUserCollaboratorIndex(string id)
        {
            var idx = 0;
            var q = (from a in Collaborations.CurrentCollaborations where a.Collaborator_Id == id select a.Index);
            var e = q as List<int> ?? q.ToList();
            if (e.Any())
                idx = e.First();
            return idx;
        }

        public static bool IsPendingAdd(int s)
        {
            return (s == (int)_Enum.Status.AuthorAdded && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.ContributorAdded && EditorState.EditContext == _Enum.EditContext.Authoring
                    || s == (int)_Enum.Status.PendingAuthorAction && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsPendingDelete(int s)
        {
            return (s == (int)_Enum.Status.AuthorDeleted && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.ContributorDeleted && EditorState.EditContext == _Enum.EditContext.Authoring
                    || s == (int)_Enum.Status.WaitingOnContributor && EditorState.EditContext == _Enum.EditContext.Contributing
                    || s == (int)_Enum.Status.WaitingOnAuthor && EditorState.EditContext == _Enum.EditContext.Authoring);
        }

        public static bool IsActive(Chord ch)
        {
            bool result = false;
            try
            {
                var a = (from n in ch.Notes
                         where (IsActionable(n, null))
                         select n);
                result = a.Any();
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = CollaborationManager method = IsActive(Repository.DataService.Chord ch)");
            }
            return result;
        }

        public static bool IsActive(Note n)
        {
            return IsActionable(n, null);
        }
    }
}
