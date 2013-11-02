using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels
{
    public class CollaborationNotificationViewModel : BaseViewModel, ICollaborationNotificationViewModel, IEventing
    {

        public CollaborationNotificationViewModel()
        {
            SubscribeEvents();
            DefineCommands();
            Notifications = new List<Notification>();
        }

        private Microsoft.Practices.Composite.Events.IEventAggregator ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        public Microsoft.Practices.Composite.Events.IEventAggregator Ea
        {
            get { return ea; }
        }

        public Repository.DataServiceRepository<Repository.DataService.Composition> repository =
            ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();

        public Repository.DataServiceRepository<Repository.DataService.Composition> Repository
        {
            get { return repository; }
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<UpdateCollaborationNotifications>().Subscribe(OnUpdateCollaborationNotifications);
        }

        public void OnUpdateCollaborationNotifications(object obj)
        {
            List<Notification> notification = new List<Notification>();
            foreach (Composer.Repository.DataService.Collaboration c in CompositionManager.Composition.Collaborations)
            {
                if (c.Collaborator_Id != Current.User.Id)
                {
                    Statistics stats = new Statistics(0,0,0,0,0);
                    foreach(Composer.Repository.DataService.Note n in Cache.Notes)
                    {
                        string[] aStatus = n.Status.Split(',');
                        int s = int.Parse(aStatus[c.Index]);
                        if (EditorState.IsAuthor)
                        {
                            if (s == (int)_Enum.Status.ContributorAdded)
                            {
                                stats.pendingAdds++;
                            }
                            else if (s == (int)_Enum.Status.ContributorDeleted)
                            {
                                stats.pendingDeletes++;
                            }
                            else if (s == (int)_Enum.Status.ContributorAccepted)
                            {
                                stats.acceptedAddsDeletes++;
                            }
                            else if (s == (int)_Enum.Status.ContributorRejectedAdd)
                            {
                                stats.rejectedAdds++;
                            }
                            else if (s == (int)_Enum.Status.ContributorRejectedDelete)
                            {
                                stats.rejectedDeletes++;
                            }
                        }
                        else
                        {
                            if (c.Index == 0)
                            {
                                if (s == (int)_Enum.Status.AuthorAdded)
                                {
                                    stats.pendingAdds++;
                                }
                                else if (s == (int)_Enum.Status.AuthorDeleted)
                                {
                                    stats.pendingDeletes++;
                                }
                                else if (s == (int)_Enum.Status.AuthorAccepted)
                                {
                                    stats.acceptedAddsDeletes++;
                                }
                                else if (s == (int)_Enum.Status.AuthorRejectedAdd)
                                {
                                    stats.rejectedAdds++;
                                }
                                else if (s == (int)_Enum.Status.AuthorRejectedDelete)
                                {
                                    stats.rejectedDeletes++;
                                }
                            }
                        }
                    }
                    notification.Add(new Notification(c, stats));
                }
            }
            Notifications = notification;
        }

        public void DefineCommands()
        {

        }

        private List<Notification> _notifications = null;
        public List<Notification> Notifications
        {
            get { return _notifications; }
            set
            {
                _notifications = value;
                OnPropertyChanged(() => Notifications);
            }
        }

        private string _margin = "50,350,0,0";
        public string Margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                OnPropertyChanged(() => Margin);
            }
        }

        private string _background = "Beige";
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        public struct Statistics
        {
            public int pendingAdds;
            public int pendingDeletes;
            public int acceptedAddsDeletes;
            public int rejectedAdds;
            public int rejectedDeletes;

            public Statistics(int p1, int p2, int p3, int p4, int p5)
            {
                pendingAdds = p1;
                pendingDeletes = p2;
                acceptedAddsDeletes = p3;
                rejectedAdds = p4;
                rejectedDeletes = p5;
            }
        }
    }
}
