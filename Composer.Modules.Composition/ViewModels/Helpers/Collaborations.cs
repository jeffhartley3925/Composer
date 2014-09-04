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
        private static DataServiceRepository<Repository.DataService.Composition> repository;
        private static readonly IEventAggregator Ea;
        private static Guid cOiD = Guid.Empty;
		private static readonly string AuthorOriginal = ((int)_Enum.Status.AuthorOriginal).ToString(CultureInfo.InvariantCulture);

        public static Collaborator CurrentCollaborator { get; set; }

        public static Repository.DataService.Collaboration Collaboration = null;
        public static List<Collaboration> CurrentCollaborations = new List<Collaboration>();
        public static List<Collaboration> AllCollaborations = new List<Collaboration>();
        public static List<Collaborator> Collaborators = new List<Collaborator>();

        private static readonly List<string> AuthorIds = new List<string>();
        public static int Index { get; set; }
        private static int u = Current.User.Index;
        static Collaborations()
        {
            if (repository == null)
            {
                repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                SubscribeEvents();
            }
        }

        public static void SubscribeEvents()
        {
        }

        public static void Initialize(Guid compositionId, string fbId)
        {
            cOiD = compositionId;
            GetCollaborations();
        }

        private static void GetCollaborations()
        {
            Collaboration cN;
            AllCollaborations = new List<Collaboration>();
            Collaborators = new List<Collaborator>();
            foreach (var o in CompositionManager.Composition.Collaborations)
            {
                if (o.Collaborator_Id == Current.User.Id)
                {
                    Collaboration = o;
                }

                cN = new Collaboration
                                    {
                                        Key = o.Id,
                                        Name = o.Name,
                                        Composition_Id = cOiD,
                                        Author_Id = o.Author_Id,
                                        CollaboratorId = o.Collaborator_Id,
                                        Index = o.Index
                                    };
                AllCollaborations.Add(cN);
            }

            //TODO sort so we only need to do 1 of the loops below
            foreach (var c in AllCollaborations)
            {
                if (c.Index == 0)
                {
                    cN = new Collaboration { Key = c.Key, Composition_Id = cOiD, Author_Id = c.Author_Id, CollaboratorId = c.CollaboratorId, Index = c.Index };
                    CurrentCollaborations.Add(cN);
                    AuthorIds.Add(cN.CollaboratorId);
                }
            }

            foreach (var c in AllCollaborations)
            {
                if (c.Index == 0) continue;

                cN = new Collaboration { Key = c.Key, Composition_Id = cOiD, Author_Id = c.Author_Id, CollaboratorId = c.CollaboratorId, Index = c.Index };
                CurrentCollaborations.Add(cN);
                AuthorIds.Add(cN.CollaboratorId);
            }

            foreach (var c in AllCollaborations)
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

        public static int? GetStatus(Repository.DataService.Note nT)
        {
            return GetStatus(nT, Index);
        }

        public static int? GetStatus(Repository.DataService.Note nT, int cRiX)
        {
            int? result = null;
            try
            {
                var sSs = nT.Status.Split(',');
                if (sSs.Length < Index + 1)
                {
                    var sS = (sSs[0] == AuthorOriginal) ? (int)_Enum.Status.AuthorOriginal : (int)_Enum.Status.Null;

                    if (nT.Audit.Author_Id != Current.User.Id && nT.Audit.Author_Id != CompositionManager.Composition.Audit.Author_Id)
                    {
                        sS = (int)_Enum.Status.Null;
                    }
                    nT.Status = string.Format("{0},{1}", nT.Status, sS);
                    if (repository == null)
                    {
                        repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                    }
                    repository.Update(nT);
                    sSs = nT.Status.Split(',');
                }
                result = int.Parse(sSs[cRiX]);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = GetStatus(Repository.DataService.Note note, int collaboratorIndex)");
            }
            return result;
        }

        public static string SetStatus(Repository.DataService.Note nT, short sS)
        {
            return SetStatus(nT, sS, Index);
        }
        
        public static string SetStatus(Repository.DataService.Note nT, short sS, int cRiX)
        {
            string sSs = string.Empty;
            
            try
            {
                if (cRiX == 0)
                {
                    sSs = (!CollaborationManager.IsAuthorStatusActive(sS)) ?
						string.Format("{0}", (int)_Enum.Status.Null) : AuthorOriginal;

                    foreach (var collaborator in Collaborators)
                    {
                        sSs += string.Format(",{0}", sS);
                    }
                }
                else
                {
                    sSs = nT.Status;
                    if (string.IsNullOrEmpty(sSs))
                    {
						sSs = AuthorOriginal;
                    }
                    else
                    {
                        string[] collaboratorStatusTokens = sSs.Split(',');
                        sSs = string.Empty;
                        for (var i = 0; i <= collaboratorStatusTokens.Length - 1; i++)
                        {
                            sSs += (cRiX == i) ? string.Format("{0},", sS) : string.Format("{0},", collaboratorStatusTokens[i]);
                        }
                        sSs = sSs.Substring(0, sSs.Length - 1);
                    }
                }
                Ea.GetEvent<UpdateNote>().Publish(nT);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = SetStatus(Repository.DataService.Note note, short status, int collaboratorIndex)");
            }
            return CompressStatusList(sSs);
        }

        private static string CompressStatusList(string s) //comma delimited string
        {
            //TODO: This is a hack. Need to fix the real problem.
            var sSs = s.Split(',');
            if (sSs.Length > Collaborators.Count())
            {
                s = string.Format("{0}", sSs[0]);
                for (var i = 1; i <= sSs.Length - 2; i++)
                {
	                var sS = sSs[i];
                    s += string.Format(",{0}", sS);
                }
            }
            return s;
        }

        public static string SetAuthorStatus(Repository.DataService.Note nT, short status)
        {
            var sSs = string.Empty;
            try
            {
                var collaboratorStatusTokens = nT.Status.Split(',');
                //var statusToken = (EditorState.Purgable) ? status : (CollaborationManager.IsAuthorStatusActive(status) ? (int)_Enum.Status.AuthorOriginal : (int)_Enum.Status.Null);
	            var statusToken = status;
				for (var i = 0; i <= collaboratorStatusTokens.Length - 1; i++)
                {
                    sSs += (i == 0) ? string.Format("{0}", statusToken) : string.Format(",{0}", collaboratorStatusTokens[i]);
                }
                Ea.GetEvent<UpdateNote>().Publish(nT);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex, "class = Collaborations method = SetAuthorStatus(Repository.DataService.Note note, short status)");
            }
            return sSs;
        }

        public static string ConvertStatusCodesToStatusNames(string sSs)
        {
            string result = "";
            foreach (string sS in sSs.Split(','))
            {
				switch (int.Parse(sS))
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
                    case (int)_Enum.Status.Null:
                        result += " - "+_Enum.Status.Null.ToString();
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
