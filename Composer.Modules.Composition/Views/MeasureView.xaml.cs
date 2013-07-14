using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using System.Windows.Media;
using Composer.Infrastructure.Support;
using System.Collections.ObjectModel;
using System;
using Microsoft.Windows;

namespace Composer.Modules.Composition.Views
{
    public partial class MeasureView : UserControl, IMeasureView
    {
        private MeasureViewModel vm;

        public string MeasureId
        {
            get { return (string)GetValue(MeasureIdProperty); }
            set
            {
                if ((string)GetValue(MeasureIdProperty) != value)
                {
                    SetValue(MeasureIdProperty, value);
                    OnPropertyChanged("MeasureId");
                }
            }
        }

        public MeasureView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MeasureIdProperty =
            DependencyProperty.Register("MeasureId", typeof(string), typeof(MeasureView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;
            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                if (!string.IsNullOrEmpty(this.MeasureId))
                {
                    vm = new MeasureViewModel(this.MeasureId);
                    vm.View = this;
                    _ViewModels.measures.Add(vm);
                    this.DataContext = vm;
                }
            }
        }

        private void Choice0_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedForeground"]).Color);
            b.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedBackground"]).Color);
            CollaboratingChoices.Opacity = 1;
            Choice5.Opacity = 1;
            Choice7.Opacity = 1;
        }

        private void Choice0_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkForeground"]).Color);
            b.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkBackground"]).Color);
            CollaboratingChoices.Opacity = Preferences.MeasureFooterOpacity;
            Choice5.Opacity = Preferences.MeasureFooterOpacity;
            Choice7.Opacity = Preferences.MeasureFooterOpacity;
        }

        private void MeasureElement_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}