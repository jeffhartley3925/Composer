using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Modules.Composition.ViewModels
{
    public class CollaborationNotificationViewModel : BaseViewModel, ICollaborationNotificationViewModel, IEventing
    {

        public CollaborationNotificationViewModel()
        {
            SubscribeEvents();
            DefineCommands();
            Notifications = new List<Notification>();
            //Notifications.Add(new Notification("Jeff Hartley", 0, "112233"));
            //Notifications.Add(new Notification("Jim Jones", 1, "475935337"));
            //Notifications.Add(new Notification("Robye Faseler", 2, "10010299209"));

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
            List<Notification> n = new List<Notification>();
            foreach (Composer.Repository.DataService.Collaboration c in CompositionManager.Composition.Collaborations)
            {
                n.Add(new Notification(c));
            }
            Notifications = n;
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
    }
}
