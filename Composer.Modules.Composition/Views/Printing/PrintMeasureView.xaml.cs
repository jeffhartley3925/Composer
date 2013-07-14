using System.Linq;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.ComponentModel;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintMeasureView : IPrintMeasureView
	{
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

		public PrintMeasureView()
		{
			InitializeComponent();
			ServiceLocator.Current.GetInstance<IEventAggregator>();
		}

		public static readonly DependencyProperty MeasureIdProperty =
			DependencyProperty.Register("MeasureId", typeof(string), typeof(PrintMeasureView), new PropertyMetadata("", null));

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string name)
		{
			var ph = PropertyChanged;
			if (ph != null)
				ph(this, new PropertyChangedEventArgs(name));
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
		    if (DesignerProperties.IsInDesignTool) return;
		    if (!string.IsNullOrEmpty(MeasureId))
		    {
		        var viewModel = (from a in _ViewModels.measures where a.Measure.Id.ToString() == MeasureId select a).DefaultIfEmpty(null).Single();
		        DataContext = viewModel;
		    }
		}
	}
}
