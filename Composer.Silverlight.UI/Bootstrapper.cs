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
using Microsoft.Practices.Composite.UnityExtensions;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Composer.Repository.DataService;
using Composer.Modules.Composition.Service;
using Composer.Infrastructure;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.Views;
using Composer.Repository;

namespace Composer.Silverlight.UI
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            var shell = new Shell();
            Application.Current.RootVisual = shell;
            return shell;
        }
        protected override void InitializeModules()
        {
            base.InitializeModules();
        }
        protected override void ConfigureContainer()
        {

            base.ConfigureContainer();
            Unity.Container = Container;
            Container.RegisterInstance(typeof(CDataEntities), new CDataEntities(new Uri(DataServiceUri.GetUri())), new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(IEventAggregator), new EventAggregator(), new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(IModalDialogService), new ModalDialogService());
            Container.RegisterInstance(typeof(IMessageBoxService), new MessageBoxService());
        }
        protected override IModuleCatalog GetModuleCatalog()
        {
            ModuleCatalog catalog = new ModuleCatalog();
            catalog = ModuleCatalog.CreateFromXaml(new Uri("/Composer.Silverlight.UI;component/ModuleCatalog.xaml", UriKind.Relative));
            return catalog;
        }
    }
}
