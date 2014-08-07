using System.ComponentModel;
using System.Windows;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class MeasuregroupView : IMeasuregroupView
    {
        public string MeasuregroupId
        {
            get
            {
                return (string)GetValue(MeasuregroupIdProperty);
            }
            set
            {
                if ((string)GetValue(MeasuregroupIdProperty) != value)
                {
                    SetValue(MeasuregroupIdProperty, value);
                    OnPropertyChanged("MeasuregroupId");
                }
            }
        }

        public MeasuregroupView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MeasuregroupIdProperty =
            DependencyProperty.Register("MeasuregroupId", typeof(string), typeof(MeasuregroupView), new PropertyMetadata("", null));

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
                MeasuregroupViewModel viewModel = new MeasuregroupViewModel(this.MeasuregroupId);
				this.DataContext = viewModel;
            }
        }
    }
}
