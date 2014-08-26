using System.Windows;
using Composer.Modules.Composition.ViewModels;
using System.ComponentModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
	using System.Windows.Controls;

	public partial class NoteView : INoteView
    {
        private static IEventAggregator _ea;

		public Grid _dispositionContainer = null;
		public Grid DispositionContainer
		{
			get
			{
				if (_dispositionContainer == null)
				{
					_dispositionContainer = (Grid)LayoutRoot.FindName("Disposition");
				}
				return _dispositionContainer;
			}
			set
			{
				this._dispositionContainer = value;
			}
		}
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

        public NoteView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NoteIdProperty =
            DependencyProperty.Register("NoteId", typeof(string), typeof(NoteView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        NoteViewModel _viewModel;

		private void AddDispositionControl(string NoteId)
		{
			var view = new NoteDispositionView(NoteId) { DataContext = new NoteDispositionViewModel(NoteId) };
			RemoveDispositionControl();
			if (DispositionContainer.Children.Count == 0)
			{
				DispositionContainer.Children.Add(view);
			}
		}

		private void RemoveDispositionControl()
		{
			DispositionContainer.Children.Clear();
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
				AddDispositionControl(NoteId);
                _viewModel = new NoteViewModel(NoteId);
				_ViewModels.notes.Add(_viewModel);
				DataContext = _viewModel;
            }
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }
    }
}