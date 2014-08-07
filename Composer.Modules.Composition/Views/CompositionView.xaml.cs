using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System;

namespace Composer.Modules.Composition.Views
{
    public partial class CompositionView : Canvas, ICompositionView, INotifyPropertyChanged
    {
        CompositionViewModel viewModel;
        private static IEventAggregator ea;

        public CompositionView(CompositionViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.viewModel.CompositionGrid = RootGrid;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.viewModel;
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            ScriptObject screen = (ScriptObject)HtmlPage.Window.GetProperty("screen");
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        public static double ClientWidth
        {
            get
            {
                return (double)HtmlPage.Document.Body.GetProperty("clientWidth");
            }
        }

        public static double ClientHeight
        {
            get
            {
                return (double)HtmlPage.Document.Body.GetProperty("clientHeight");
            }
        }

        public static double ScreenPositionTop
        {
            get
            {
                return (double)HtmlPage.Window.GetProperty("screenTop");
            }
        }

        public static double ScreenPositionLeft
        {
            get
            {
                return (double)HtmlPage.Window.GetProperty("screenLeft");
            }
        }

        public void Canvas_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string key = e.Key.ToString();
            ea.GetEvent<KeyDown>().Publish(key);
        }

        public void Canvas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string key = e.Key.ToString();
            ea.GetEvent<KeyUp>().Publish(key);
        }

        ScrollBar verticalScrollBar = null;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)sender;
            verticalScrollBar = ((FrameworkElement)VisualTreeHelper.GetChild(Scroller, 0)).FindName("VerticalScrollBar") as ScrollBar;
            verticalScrollBar.ValueChanged += (s, ev) => TrackOffset(); 
        }

        private void TrackOffset()
        {
            double hoffset = 0;
            var voffset = verticalScrollBar.Value;
            Scroller.ScrollToVerticalOffset(voffset);
            ea.GetEvent<UpdateScrollOffset>().Publish(new Tuple<double, double>(hoffset, voffset));
        }

        //http:/kodierer.blogspot.com/2009/11/convert-encode-and-decode-silverlight.html
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            
        }
    }
}