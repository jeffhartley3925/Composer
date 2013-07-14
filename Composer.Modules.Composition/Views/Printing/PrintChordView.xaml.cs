using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintChordView : UserControl, IPrintChordView
	{
		public string ChordId
		{
			get
			{
				return (string)GetValue(ChordIdProperty);
			}
			set
			{
				SetValue(ChordIdProperty, value);
				OnPropertyChanged("ChordId");
			}
		}

		public PrintChordView()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ChordIdProperty =
			DependencyProperty.Register("ChordId", typeof(string), typeof(PrintChordView), new PropertyMetadata("", null));

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string name)
		{
			var ph = PropertyChanged;

			if (ph != null)
				ph(this, new PropertyChangedEventArgs(name));
		}

		private void UserControlLoaded(object sender, RoutedEventArgs e)
		{
			if (!DesignerProperties.IsInDesignTool)
			{
                if (!string.IsNullOrEmpty(ChordId))
                {
                    ChordViewModel viewModel = (from a in _ViewModels.chords where a.Chord.Id.ToString() == ChordId select a).DefaultIfEmpty(null).Single();
                    DataContext = viewModel;
                }
			}
		}
	}
}
