using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
    public partial class ProvenanceView : UserControl, IProvenanceView
    {
        private static IEventAggregator ea;

        public ProvenanceView()
        {
            InitializeComponent();
            
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            Loaded += new RoutedEventHandler(ProvenanceView_Loaded);
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            ea.GetEvent<ShowProvenancePanel>().Subscribe(OnShowProvenancePanel);
        }

        public void OnShowProvenancePanel(object obj)
        {
            this.TitleLine1.SelectAll();
        }

        void ProvenanceView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                IProvenanceViewModel viewModel = (ProvenanceViewModel)ServiceLocator.Current.GetInstance<ProvenanceViewModel>();
                this.DataContext = viewModel;
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Border b = (Border)sender;
            double h = b.ActualHeight;
        }
    }
}
