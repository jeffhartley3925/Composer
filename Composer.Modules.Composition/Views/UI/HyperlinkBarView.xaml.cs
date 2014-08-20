using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Composer.Modules.Composition.ViewModels.UI;

namespace Composer.Modules.Composition.Views
{
    public partial class HyperlinkBarView : UserControl, IHyperlinkBarView
    {
        HyperlinkBarViewModel vM;
        public HyperlinkBarView(HyperlinkBarViewModel vM)
        {
            this.vM = vM;
            InitializeComponent();
        }

        private void HyperlinkBar_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this.vM;
        }

        private void HyperlinkButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            btn.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedForeground"]).Color);
            btn.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkSelectedBackground"]).Color);
        }

        private void HyperlinkButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            btn.Foreground = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkButtonForeground"]).Color);
            btn.Background = new SolidColorBrush(((SolidColorBrush)Application.Current.Resources["HyperlinkButtonBackground"]).Color);
        }
    }
}
