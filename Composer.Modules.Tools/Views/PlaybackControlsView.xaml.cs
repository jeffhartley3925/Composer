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
using Composer.Modules.Palettes.ViewModels;

namespace Composer.Modules.Palettes.Views
{
    public partial class PlaybackControlsView : Canvas, IPlaybackControlsView
    {
        PlaybackViewModel ViewModel;
        public PlaybackControlsView(PlaybackViewModel viewModel)
        {
            InitializeComponent();
            this.ViewModel = viewModel;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.ViewModel;
            this.LayoutRoot.Visibility = Visibility.Visible;
        }

        bool isMouseCaptured;
        double mouse_Y;
        double mouse_X;

        public void Handle_MouseDown(object sender, MouseEventArgs args)
        {
            Canvas item = sender as Canvas;
            mouse_Y = args.GetPosition(null).Y;
            mouse_X = args.GetPosition(null).X;
            isMouseCaptured = true;
            item.CaptureMouse();
        }
        public void Handle_MouseMove(object sender, MouseEventArgs args)
        {
            Canvas item = sender as Canvas;
            if (isMouseCaptured)
            {
                // Calculate the current position of the object.
                double deltaV = args.GetPosition(null).Y - mouse_Y;
                double deltaH = args.GetPosition(null).X - mouse_X;
                double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                // Set new position of object.
                item.SetValue(Canvas.TopProperty, newTop);
                item.SetValue(Canvas.LeftProperty, newLeft);

                // Update position global variables.
                mouse_Y = args.GetPosition(null).Y;
                mouse_X = args.GetPosition(null).X;
            }
        }
        public void Handle_MouseUp(object sender, MouseEventArgs args)
        {
            Canvas item = sender as Canvas;
            isMouseCaptured = false;
            item.ReleaseMouseCapture();
            mouse_Y = -1;
            mouse_X = -1;
        }
    }
}
