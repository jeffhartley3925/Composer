using System.Windows;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Composer.Modules.Composition.ViewModels;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
    public partial class SavePanelView : UserControl, ISavePanelView
    {
        private static IEventAggregator ea;
        public SavePanelView()
        {
            InitializeComponent();
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                IUnityContainer container = Unity.Container;
                if (!container.IsRegistered<ISavePanelViewModel>())
                {
                    container.RegisterType<ISavePanelViewModel, SavePanelViewModel>(new ContainerControlledLifetimeManager());
                }
                var viewModel = (ISavePanelViewModel)ServiceLocator.Current.GetInstance<ISavePanelViewModel>() ??
                                (ISavePanelViewModel)container.Resolve<ISavePanelViewModel>();
                DataContext = viewModel;
                ea.GetEvent<HideSavePanel>().Publish(string.Empty);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
