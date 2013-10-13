using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public static class Collaborations
    {
        public static List<DispositionChangeItem> DispositionChanges = null;
        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        private static readonly IEventAggregator Ea;
        private static Guid _compositionId = Guid.Empty;

        public static Collaborator CurrentCollaborator { get; set; }
        public static Composer.Repository.DataService.Collaboration COLLABORATION = null;
        public static List<Collaboration> CurrentCollaborations = new List<Collaboration>();
        public static List<Collaboration> AllCollaborations = new List<Collaboration>();
        public static List<Collaborator> Collaborators = new List<Collaborator>();

        private static readonly List<string> AuthorIds = new List<string>();
        public static int Index { get; set; }

        static Collaborations()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        public static void SubscribeEvents()
        {
        }

        public static void Initialize(Guid compositionId, string fbId)
        {
            _compositionId = compositionId;
            GetCollaborations();
        }

        private static void GetCollaborations()
        {
            Collaboration collaboration;
            AllCollaborations = new List<Collaboration>();
            Collaborators = new List<Collaborator>();
            foreach (var o in CompositionManager.Composition.Collaborations)
            {
                if (o.Collaborator_Id.ToString() == Current.User.Id)
                {
                    COLLABORATION = o;
                }

                collaboration = new Collaboration
                                    {
                                        Key = o.Id,
                                        Name = o.Name,
                                        Composition_Id = _compositionId,
                                        Author_Id = o.Author_Id,
                                        CollaboratorId = o.Collaborator_Id,
                                        Index = o.Index
                                    };
                AllCollaborations.Add(collaboration);
            }

            //TODO sort so we only need to do 1 of the loops below
            foreach (var c in AllCollaborations)
            {
                if (c.Index == 0)
                {
                    collaboration = new Collaboration { Key = c.Key, Composition_Id = _compositionId, Author_Id = c.Author_Id, CollaboratorId = c.CollaboratorId, Index = c.Index };
                    CurrentCollaborations.Add(collaboration);
                    AuthorIds.Add(collaboration.CollaboratorId);
                }
            }

            foreach (Collaboration c in AllCollaborations)
            {
                if (c.Index <= 0) continue;

                collaboration = new Collaboration() { Key = c.Key, Composition_Id = _compositionId, Author_Id = c.Author_Id, CollaboratorId = c.CollaboratorId, Index = c.Index };
                CurrentCollaborations.Add(collaboration);
                AuthorIds.Add(collaboration.CollaboratorId);
            }

            foreach (Collaboration c in AllCollaborations)
            {
                Collaborators.Add(new Collaborator(c.Name, c.CollaboratorId));
                if (c.CollaboratorId == Current.User.Id)
                {
                    Current.User.Index = c.Index;
                }
            }
            if (EditorState.IsOpening) //new compositions have no associated images.
            {
                Ea.GetEvent<UpdatePinterestImage>().Publish(string.Empty);
                Ea.GetEvent<ShowSocialChannels>().Publish(_Enum.SocialChannel.All);
            }
            else
            {
                Ea.GetEvent<HideSocialChannels>().Publish(_Enum.SocialChannel.All);
            }
            Ea.GetEvent<UpdateProvenancePanel>().Publish(CompositionManager.Composition);
            if (Collaborators.Count > 1)
            {
                Ea.GetEvent<UpdateCollaborationNotifications>().Publish(string.Empty);
            }
        }

        public static int GetStatus(Repository.DataService.Note note)
        {
            return GetStatus(note, Index);
        }

        public static int GetStatus(Repository.DataService.Note note, int index)
        {
            var result = -1;
            try
            {
                var arr = note.Status.Split(',');

                if (arr.Length < Index + 1)
                {
                    var status = (arr[0] == ((int)_Enum.Status.AuthorOriginal).ToString(CultureInfo.InvariantCulture)) ?
                        (int)_Enum.Status.AuthorOriginal : (int)_Enum.Status.Meaningless;

                    if (note.Audit.Author_Id != Current.User.Id && note.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id)
                    {
                        status = (int)_Enum.Status.Meaningless;
                    }

                    note.Status = string.Format("{0},{1}", note.Status, status);

                    if (_repository == null)
                    {
                        _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                    }

                    _repository.Update(note);

                    arr = note.Status.Split(',');
                }
                result = int.Parse(arr[index]);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = GetStatus(Repository.DataService.Note note, int index)");
            }
            return result;
        }

        public static string SetStatus(Repository.DataService.Note note, short status)
        {
            return SetStatus(note, status, Index);
        }
        
        public static string SetStatus(Repository.DataService.Note note, short status, int index)
        {
            string statusList = string.Empty;
            try
            {
                if (index == 0)
                {
                    statusList = (!CollaborationManager.IsAuthorStatusActive(status)) ?
                        string.Format("{0}", (int)_Enum.Status.Meaningless) : string.Format("{0}", (int)_Enum.Status.AuthorOriginal);

                    for (var i = 1; i <= Collaborators.Count(); i++)
                    {
                        statusList += string.Format(",{0}", status);
                    }
                }
                else
                {
                    statusList = note.Status;
                    if (string.IsNullOrEmpty(statusList))
                    {
                        statusList = string.Format("{0}", (int)_Enum.Status.AuthorOriginal);
                    }
                    else
                    {
                        string[] arr = statusList.Split(',');
                        statusList = string.Empty;
                        for (var i = 0; i <= arr.Length - 1; i++)
                        {
                            statusList += (index == i) ? string.Format("{0},", status) : string.Format("{0},", arr[i]);
                        }
                        statusList = statusList.Substring(0, statusList.Length - 1);
                    }
                }
                Ea.GetEvent<UpdateNote>().Publish(note);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = SetStatus(Repository.DataService.Note note, short status, int index)");
            }
            return CompressStatusList(statusList);
        }

        private static string CompressStatusList(string cds) //comma delimited string
        {
            //TODO: This is a hack. Need to fix the real problem.
            var a = cds.Split(',');
            if (a.Length > Collaborators.Count())
            {
                cds = string.Format("{0}", a[0]);
                for (var i = 1; i <= a.Length - 2; i++)
                {
                    cds += string.Format(",{0}", a[i]);
                }
            }
            return cds;
        }

        public static string SetAuthorStatus(Repository.DataService.Note note, short status)
        {
            var result = string.Empty;
            try
            {
                var arr = note.Status.Split(',');

                var stat = (EditorState.Purgable) ? status : (CollaborationManager.IsAuthorStatusActive(status) ? (int)_Enum.Status.AuthorOriginal : (int)_Enum.Status.Meaningless);
                for (var i = 0; i <= arr.Length - 1; i++)
                {
                    result += (i == 0) ? string.Format("{0}", stat) : string.Format(",{0}", arr[i]);
                }
                Ea.GetEvent<UpdateNote>().Publish(note);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = SetAuthorStatus(Repository.DataService.Note note, short status)");
            }
            return result;
        }

        public static string ConvertStatusCodesToStatusNames(string status)
        {
            string result = "";
            string[] statusArray = status.Split(',');
            foreach (string statusItem in statusArray)
            {
                int item = int.Parse(statusItem);
                switch (item)
                {
                    case (int)_Enum.Status.AuthorAccepted:
                        result += " - "+_Enum.Status.AuthorAccepted.ToString();
                        break;
                    case (int)_Enum.Status.AuthorAdded:
                        result += " - "+_Enum.Status.AuthorAdded.ToString();
                        break;
                    case (int)_Enum.Status.AuthorDeleted:
                        result += " - "+_Enum.Status.AuthorDeleted.ToString();
                        break;
                    case (int)_Enum.Status.WaitingOnContributor:
                        result += " - "+_Enum.Status.WaitingOnContributor.ToString();
                        break;
                    case (int)_Enum.Status.AuthorModified:
                        result += " - "+_Enum.Status.AuthorModified.ToString();
                        break;
                    case (int)_Enum.Status.AuthorOriginal:
                        result += " - "+_Enum.Status.AuthorOriginal.ToString();
                        break;
                    case (int)_Enum.Status.AuthorRejectedAdd:
                        result += " - "+_Enum.Status.AuthorRejectedAdd.ToString();
                        break;
                    case (int)_Enum.Status.AuthorRejectedDelete:
                        result += " - "+_Enum.Status.AuthorRejectedDelete.ToString();
                        break;
                    case (int)_Enum.Status.ContributorAccepted:
                        result += " - "+_Enum.Status.ContributorAccepted.ToString();
                        break;
                    case (int)_Enum.Status.ContributorAdded:
                        result += " - "+_Enum.Status.ContributorAdded.ToString();
                        break;
                    case (int)_Enum.Status.ContributorDeleted:
                        result += " - "+_Enum.Status.ContributorDeleted.ToString();
                        break;
                    case (int)_Enum.Status.WaitingOnAuthor:
                        result += " - "+_Enum.Status.WaitingOnAuthor.ToString();
                        break;
                    case (int)_Enum.Status.ContributorRejectedAdd:
                        result += " - "+_Enum.Status.ContributorRejectedAdd.ToString();
                        break;
                    case (int)_Enum.Status.ContributorRejectedDelete:
                        result += " - "+_Enum.Status.ContributorRejectedDelete.ToString();
                        break;
                    case (int)_Enum.Status.Meaningless:
                        result += " - "+_Enum.Status.Meaningless.ToString();
                        break;
                    case (int)_Enum.Status.PendingAuthorAction:
                        result += " - "+_Enum.Status.PendingAuthorAction.ToString();
                        break;
                    case (int)_Enum.Status.PendingContributorAction:
                        result += " - "+_Enum.Status.PendingContributorAction.ToString();
                        break;
                    case (int)_Enum.Status.Purged:
                        result += " - "+_Enum.Status.Purged.ToString();
                        break;

                }
            }
            result = result.Substring(3);
            return result;
        }
    }
}
