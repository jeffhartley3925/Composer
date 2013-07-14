using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Unity;
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

        private void SubscribeEvents()
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
