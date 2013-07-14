using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using System.Linq;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Views
{
    public partial class StaffgroupView : UserControl, IStaffgroupView
    {
        public string StaffgroupId
        {
            get
            {
                return (string)GetValue(StaffgroupIdProperty);
            }
            set
            {
                if ((string)GetValue(StaffgroupIdProperty) != value)
                {
                    SetValue(StaffgroupIdProperty, value);
                    OnPropertyChanged("StaffgroupId");
                }
            }
        }

        public StaffgroupView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StaffgroupIdProperty =
            DependencyProperty.Register("StaffgroupId", typeof(string), typeof(StaffgroupView), new PropertyMetadata("", null));

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
                if (!string.IsNullOrEmpty(this.StaffgroupId))
                {
                    //_ViewModels used by the Print system
					if (_ViewModels.staffgroups == null)
					{
						_ViewModels.Initialize();
					}
					StaffgroupViewModel viewModel = new StaffgroupViewModel(this.StaffgroupId);
                    if (! StaffgroupManager.IsEmpty(viewModel.Staffgroup))
                    {
					    _ViewModels.staffgroups.Add(viewModel); 
                    }
					this.DataContext = viewModel;
                }
            }
        }
    }
}