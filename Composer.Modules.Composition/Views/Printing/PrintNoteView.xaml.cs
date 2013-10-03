using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintNoteView : UserControl, IPrintNoteView
	{
		public string NoteId
		{
			get
			{
				return (string)GetValue(NoteIdProperty);
			}
			set
			{
				SetValue(NoteIdProperty, value);
				OnPropertyChanged("NoteId");
			}
		}

		public PrintNoteView()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty NoteIdProperty =
			DependencyProperty.Register("NoteId", typeof(string), typeof(PrintNoteView), new PropertyMetadata("", null));

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
				NoteViewModel viewModel = (from a in _ViewModels.notes where a.Note.Id.ToString() == this.NoteId select a).DefaultIfEmpty(null).Single();
				this.DataContext = viewModel;
			}
		}
	}
}
