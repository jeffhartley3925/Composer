using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class ArcView : IArcView
    {
        public string ArcId
        {
            get
            {
                return (string)GetValue(ArcIdProperty);
            }
            set
            {
                SetValue(ArcIdProperty, value);
                OnPropertyChanged("ArcId");
            }
        }

        public ArcView()
        {
            Loaded += ArcView_Loaded;
            InitializeComponent();
        }

        void ArcView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                DataContext = new ArcViewModel(ArcId);
            }
        }

        public static readonly DependencyProperty ArcIdProperty =
            DependencyProperty.Register("ArcId", typeof(string), typeof(ArcView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}