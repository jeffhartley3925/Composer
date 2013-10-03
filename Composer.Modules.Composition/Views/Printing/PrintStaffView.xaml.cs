using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintStaffView : UserControl, IPrintStaffView
	{
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

		public PrintStaffView()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty StaffIdProperty =
			DependencyProperty.Register("StaffId", typeof(string), typeof(PrintStaffView), new PropertyMetadata("", null));

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
					StaffViewModel viewModel = (from a in _ViewModels.staffs where a.Staff.Id.ToString() == this.StaffId select a).DefaultIfEmpty(null).Single();
					this.DataContext = viewModel;
				}
			}

		}
	}
}
