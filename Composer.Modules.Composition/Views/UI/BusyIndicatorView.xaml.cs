using System.Windows;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class BusyIndicatorView : IBusyIndicatorView
    {
        public BusyIndicatorView()
        {
            InitializeComponent();
            Loaded += BusyIndicatorView_Loaded;
        }

        private void BusyIndicatorView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                DataContext = new BusyIndicatorViewModel();
            }
        }
    }
}