using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Modules.Composition.Views
{
    public partial class CollaborationNotificationView : Canvas, ICollaborationNotificationView
    {
        private CollaborationNotificationViewModel vm;
        private static IEventAggregator _ea;
        private Brush _saveBrush;
        private Grid _saveGrid;
        public CollaborationNotificationView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                vm = new CollaborationNotificationViewModel();
                this.DataContext = vm;
            }
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
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
            //_ea.GetEvent<HubCompositionMouseEnter>().Publish(Infrastructure.Support.Utilities.GetCompositionImageUriFromCompositionId(grid.Tag.ToString()));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            var grid = (Grid)sender;
            if (_saveGrid != grid)
            {
                grid.Background = _saveBrush;
            }
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
         }
    }
}
