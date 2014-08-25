using System.ComponentModel;
using System.Windows;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class SequenceView : ISequenceView
    {
        public string Sequence
        {
            get
            {
                return (string)GetValue(SequenceProperty);
            }
            set
            {
                if ((string)GetValue(SequenceProperty) != value)
                {
                    SetValue(SequenceProperty, value);
                    OnPropertyChanged("Sequence");
                }
            }
        }

        public SequenceView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register("Sequence", typeof(string), typeof(SequenceView), new PropertyMetadata("", null));

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
                SequenceViewModel viewModel = new SequenceViewModel(this.Sequence);
				this.DataContext = viewModel;
            }
        }
    }
}
