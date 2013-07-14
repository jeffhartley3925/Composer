using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Composer.Modules.Dialogs.ViewModels;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Views
{
    public partial class HubView : IHubView
    {
        private readonly HubViewModel _viewModel;
        private Brush _saveBrush;
        private Grid _saveGrid;
        private static IEventAggregator _ea;

        public HubView(HubViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Infrastructure.Support.FacebookData.Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;

            var width = ActualWidth;
            var height = ActualHeight;
            var p = new Point(width, height);

            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _ea.GetEvent<PlaceCompositionPanel>().Publish(p);
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            System.Windows.Media.Imaging.BitmapImage img = (System.Windows.Media.Imaging.BitmapImage)image.Source;
            //_ea.GetEvent<HubCompositionMouseEnter>().Publish(img.UriSource.ToString());
        }
        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            System.Windows.Media.Imaging.BitmapImage img = (System.Windows.Media.Imaging.BitmapImage)image.Source;
            //_ea.GetEvent<HubCompositionMouseLeave>().Publish(img.UriSource.ToString());
        }
        private void Image_Click(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            System.Windows.Media.Imaging.BitmapImage img = (System.Windows.Media.Imaging.BitmapImage)image.Source;
            //_ea.GetEvent<HubCompositionMouseClick>().Publish(img.UriSource.ToString());
        }
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            if (_saveGrid != grid)
            {
                _saveBrush = grid.Background;
                var color = (Color)Resources["HoverColor"];
                grid.Background = new SolidColorBrush(color);
            }
            _ea.GetEvent<HubCompositionMouseEnter>().Publish(Infrastructure.Support.Utilities.GetCompositionImageUriFromCompositionId(grid.Tag.ToString()));
        }
        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            if (_saveGrid != grid)
            {
                grid.Background = _saveBrush;
            }
            _ea.GetEvent<HubCompositionMouseLeave>().Publish(Infrastructure.Support.Utilities.GetCompositionImageUriFromCompositionId(grid.Tag.ToString()));
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var color = (Color)Resources["PressedColor"];
            var grid = (Grid)sender;
            grid.Background = new SolidColorBrush(color);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            if (_saveGrid != null)
            {
                _saveGrid.Background = _saveBrush;
            }

            if (_saveGrid != grid)
            {
                var color = (Color)Resources["ListBoxSelectedItemColor"];
                grid.Background = new SolidColorBrush(color);
                _saveGrid = grid;

                string id = grid.Tag.ToString();
                if (id.Length > 0)
                {
                    _ea.GetEvent<ForwardComposition>().Publish(id);
                }
            }
            else
            {
                _saveGrid = null;
                _ea.GetEvent<ForwardComposition>().Publish("");
            }
            _ea.GetEvent<HubCompositionMouseClick>().Publish(Infrastructure.Support.Utilities.GetCompositionImageUriFromCompositionId(grid.Tag.ToString()));
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void HubLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
