using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Views
{
    public partial class NewCompositionPanelView : INewCompositionPanelView
    {
        private readonly IEventAggregator _ea;

        public NewCompositionPanelView()
        {
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            InitializeComponent();
        }

        void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = Resources["FadeInStoryboard"] as Storyboard;
            if (fadeIn != null) fadeIn.Begin();

            var borderTracer = Resources["BorderStoryboard"] as Storyboard;
            if (borderTracer != null) borderTracer.Begin();

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                DataContext = new NewCompositionPanelViewModel();
            }
        }

        private void TitleMouseEnter(object sender, MouseEventArgs e)
        {
            _ea.GetEvent<HideNewCompositionTitleValidator>().Publish(string.Empty);
        }

        private void TitleTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text.Length > 128)
            {
                textBox.Text = textBox.Text.Substring(0, 128);
                _ea.GetEvent<ShowNewCompositionTitleValidator>().Publish(Infrastructure.Constants.Messages.NewCompositionPanelTitleLengthPrompt);
            }
            if (textBox.Text.IndexOf(Infrastructure.Constants.Messages.NewCompositionPanelTitlePrompt, StringComparison.Ordinal) >= 0 || textBox.Text.Trim().Length == 0)
            {
                _ea.GetEvent<SetNewCompositionTitleForeground>().Publish(Preferences.DisabledColor);
            }
            else
            {
                _ea.GetEvent<SetNewCompositionTitleForeground>().Publish(Preferences.ProvenanceForeground);
            }
        }

        private void TitleGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text.IndexOf(Infrastructure.Constants.Messages.NewCompositionPanelTitlePrompt, StringComparison.Ordinal) >= 0)
            {
                textBox.SelectAll();
            }
        }

        private void SimpleStaffRadioChecked(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<NewCompositionPanelStaffConfigurationChanged>().Publish(_Enum.StaffConfiguration.Simple);
        }

        private void GrandStaffRadioChecked(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<NewCompositionPanelStaffConfigurationChanged>().Publish(_Enum.StaffConfiguration.Grand);
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
