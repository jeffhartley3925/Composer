using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Infrastructure;
using Composer.Modules.Composition.ViewModels;
using System.Windows.Media;

namespace Composer.Modules.Composition.Views
{
    public partial class MeasureView : IMeasureView
    {
        private MeasureViewModel _vm;

        public string MeasureId
        {
            get { return (string)GetValue(MeasureIdProperty); }
            set
            {
                if ((string)GetValue(MeasureIdProperty) != value)
                {
                    SetValue(MeasureIdProperty, value);
                    OnPropertyChanged("MeasureId");
                }
            }
        }

        public MeasureView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MeasureIdProperty =
            DependencyProperty.Register("MeasureId", typeof(string), typeof(MeasureView), new PropertyMetadata("", null));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            var ph = PropertyChanged;
            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool) return;
            if (string.IsNullOrEmpty(MeasureId)) return;
            _vm = new MeasureViewModel(MeasureId) {View = this};
            _ViewModels.measures.Add(_vm);
            DataContext = _vm;
        }

        private void Choice0_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var b = (Button)sender;
            b.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedForeground"]).Color);
            b.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedBackground"]).Color);
            CollaboratingChoices.Opacity = 1;
            Choice5.Opacity = 1;
            Choice7.Opacity = 1;
        }

        private void Choice0_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button b = (Button)sender;
            b.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkForeground"]).Color);
            b.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkBackground"]).Color);
            CollaboratingChoices.Opacity = Preferences.MeasureFooterOpacity;
            Choice5.Opacity = Preferences.MeasureFooterOpacity;
            Choice7.Opacity = Preferences.MeasureFooterOpacity;
        }

        private void MeasureElement_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}