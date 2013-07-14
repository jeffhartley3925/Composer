using System.Windows;
using Composer.Modules.Composition.ViewModels;
using System.ComponentModel;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.Views
{
    public partial class NoteView : INoteView
    {
        private static IEventAggregator _ea;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                _viewModel = new NoteViewModel(NoteId);
				_ViewModels.notes.Add(_viewModel);
				DataContext = _viewModel;
            }
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<RejectClick>().Publish(_viewModel.Note.Id);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<AcceptClick>().Publish(_viewModel.Note.Id);
        }
    }
}
