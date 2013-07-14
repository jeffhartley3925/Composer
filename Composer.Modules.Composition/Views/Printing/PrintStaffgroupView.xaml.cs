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
using System.ComponentModel;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintStaffgroupView : UserControl, IPrintStaffgroupView
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

	   public PrintStaffgroupView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StaffgroupIdProperty =
            DependencyProperty.Register("StaffgroupId", typeof(string), typeof(PrintStaffgroupView), new PropertyMetadata("", null));

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
					StaffgroupViewModel viewModel = (from a in _ViewModels.staffgroups where a.Staffgroup.Id.ToString() == this.StaffgroupId select a).DefaultIfEmpty(null).Single();
					this.DataContext = viewModel;
                }
            }
        }
	}
}
