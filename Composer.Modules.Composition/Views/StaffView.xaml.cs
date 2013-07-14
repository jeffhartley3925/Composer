using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using System.Windows.Media;
using System.Windows.Shapes;
using Composer.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
    public partial class StaffView : UserControl, IStaffView
    {
        private IEventAggregator ea;

        public string StaffId
        {
            get
            {
                return (string)GetValue(StaffIdProperty);
            }
            set
            {
                SetValue(StaffIdProperty, value);
                OnPropertyChanged("StaffId");
            }
        }

        public StaffView()
        {
            InitializeComponent();
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public static readonly DependencyProperty StaffIdProperty =
            DependencyProperty.Register("StaffId", typeof(string), typeof(StaffView), new PropertyMetadata("", null));

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
                if (!string.IsNullOrEmpty(this.StaffId))
                {
					StaffViewModel viewModel = new StaffViewModel(this.StaffId);
					_ViewModels.staffs.Add(viewModel);
					this.DataContext = viewModel;
                    ea.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
                }
            }
        }

        private void HyperlinkButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            btn.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedForeground"]).Color);
            btn.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedBackground"]).Color);
        }

        private void HyperlinkButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            btn.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkForeground"]).Color);
            btn.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkBackground"]).Color);
        }
    }
}