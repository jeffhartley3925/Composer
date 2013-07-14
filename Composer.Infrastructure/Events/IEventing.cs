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

namespace Composer.Infrastructure.Events
{
    public interface IEventing
    {
        IEventAggregator Ea
        {
            get;
        }

        DataServiceRepository<Repository.DataService.Composition> Repository
        {
            get;
        }

        void SubscribeEvents();
        void DefineCommands();
    }
}
