using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Modules.Composition.ViewModels.UI
{
    public class CollaborationStatisticsViewModel : BaseViewModel, ICollaborationStatisticsViewModel, IEventing
    {
        public CollaborationStatisticsViewModel()
        {
            SubscribeEvents();
            DefineCommands();
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

        }

        public void DefineCommands()
        {

        }
    }
}
