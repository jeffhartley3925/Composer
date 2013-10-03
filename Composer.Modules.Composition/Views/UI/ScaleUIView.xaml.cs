using System;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
    public partial class UIScaleView : UserControl, IUIScaleView
    {
        UIScaleViewModel viewModel;
        double start, end;

        public UIScaleView(UIScaleViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            start = UIScaleCtrl.Minimum;
            end = UIScaleCtrl.Maximum;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.viewModel;
            txtScalePercent.Text = "100%";

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double rawScale = e.NewValue;
            double scale = Math.Round(e.NewValue * 100, 0);
            if (Math.Abs(71 - scale) <= 3)
            {
                rawScale = .71;
                scale = 71;
            }
            if (txtScalePercent != null)
            {
                txtScalePercent.Text = string.Format("{0}%", scale);
                IEventAggregator _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                _ea.GetEvent<ScaleViewportChanged>().Publish(rawScale);
            }
        }

        private void Full_Click(object sender, RoutedEventArgs e)
        {
            double rawScale = 1;
            double scale = 100;
            if (txtScalePercent != null)
            {
                txtScalePercent.Text = string.Format("{0}%", scale);
                IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<ScaleViewportChanged>().Publish(rawScale);
            }
        }

        private void Fill_Click(object sender, RoutedEventArgs e)
        {
            double rawScale = .71;
            double scale = 71;
            if (txtScalePercent != null)
            {
                txtScalePercent.Text = string.Format("{0}%", scale);
                IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<ScaleViewportChanged>().Publish(rawScale);
            }
        }
    }
}
