using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using System.Windows.Media;
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

		public string StaffIndex
		{
			get
			{
				return (string)GetValue(StaffIndexProperty);
			}
			set
			{
				SetValue(StaffIndexProperty, value);
				OnPropertyChanged("StaffIndex");
			}
		}

		public string StaffgroupId
		{
			get
			{
				return (string)GetValue(StaffgroupIdProperty);
			}
			set
			{
				SetValue(StaffgroupIdProperty, value);
				OnPropertyChanged("StaffgroupId");
			}
		}

        public StaffView()
        {
            InitializeComponent();
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public static readonly DependencyProperty StaffIdProperty =
            DependencyProperty.Register("StaffId", typeof(string), typeof(StaffView), new PropertyMetadata("", null));

		public static readonly DependencyProperty StaffIndexProperty =
			DependencyProperty.Register("StaffIndex", typeof(string), typeof(StaffView), new PropertyMetadata("", null));

		public static readonly DependencyProperty StaffgroupIdProperty =
			DependencyProperty.Register("StaffgroupId", typeof(string), typeof(StaffView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                if (!string.IsNullOrEmpty(this.StaffId))
                {
					StaffViewModel viewModel = new StaffViewModel(this.StaffId, this.StaffgroupId, this.StaffIndex);
					_ViewModels.staffs.Add(viewModel);
					this.DataContext = viewModel;
                    ea.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
                }
            }
        }
    }
}