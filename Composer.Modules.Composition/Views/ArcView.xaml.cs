using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class ArcView : Canvas, IArcView
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
            this.Loaded += new RoutedEventHandler(ArcView_Loaded);
            InitializeComponent();
        }

        void ArcView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.DataContext = new ArcViewModel(this.ArcId);
            }
        }

        public static readonly DependencyProperty ArcIdProperty =
            DependencyProperty.Register("ArcId", typeof(string), typeof(ArcView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void Canvas_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}