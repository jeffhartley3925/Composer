using System.Windows;

namespace Composer.Modules.Composition.Views
{
	using System;
	using System.ComponentModel;

	using Composer.Infrastructure.Events;

	using Microsoft.Practices.Composite.Events;
	using Microsoft.Practices.ServiceLocation;

	public partial class NoteDispositionView : INoteDispositionView
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

        public NoteDispositionView(string nTiD)
        {
	        NoteId = nTiD;
            InitializeComponent();
        }

        public static readonly DependencyProperty NoteIdProperty =
            DependencyProperty.Register("NoteId", typeof(string), typeof(NoteDispositionView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            var ph = PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
				//DataContext = new NoteDispositionViewModel(NoteId);
            }
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<RejectClick>().Publish(Guid.Parse(NoteId));
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
			_ea.GetEvent<AcceptClick>().Publish(Guid.Parse(NoteId));
        }
	}
}
