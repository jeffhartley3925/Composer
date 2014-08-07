using System.ComponentModel;
using System.Windows;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Views
{
    public partial class SequenceView : ISequenceView
    {
        public string SequenceIndex
        {
            get
            {
                return (string)GetValue(SequenceIndexProperty);
            }
            set
            {
                if ((string)GetValue(SequenceIndexProperty) != value)
                {
                    SetValue(SequenceIndexProperty, value);
                    OnPropertyChanged("SequenceIndex");
                }
            }
        }

        public SequenceView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SequenceIndexProperty =
            DependencyProperty.Register("SequenceIndex", typeof(string), typeof(SequenceView), new PropertyMetadata("", null));

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
                SequenceViewModel viewModel = new SequenceViewModel(this.SequenceIndex);
				this.DataContext = viewModel;
            }
        }
    }
}
