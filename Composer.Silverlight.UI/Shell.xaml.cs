using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Browser;
using System.Windows.Printing;
using System;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;

namespace Composer.Silverlight.UI
{
    public partial class Shell : UserControl
    {
        private static IEventAggregator _ea;
        public Shell()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UserControl_Loaded);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.DataContext = new ShellViewModel();
            }
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            JavascriptSilverlightBridge JStoSLBridge = new JavascriptSilverlightBridge();
            HtmlPage.RegisterScriptableObject("ContribShell", JStoSLBridge);
            HtmlDocument htmlDoc = HtmlPage.Document;

            HtmlPage.Window.Invoke("onReadySignal");

        }

        private void CompositionPanelContent_Loaded(object sender, RoutedEventArgs e)
        {
            ContentControl contentControl = (ContentControl)sender;
            contentControl.Visibility = Visibility.Visible;
        }

        private void ShellUserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _ea.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
        }
    }
}