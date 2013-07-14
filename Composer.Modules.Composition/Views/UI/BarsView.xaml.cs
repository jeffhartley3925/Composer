using System.Windows;
using System.Windows.Input;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.Views
{
    public partial class BarsView : IBarsView
    {
        public BarsView()
        {
            Loaded += BarsView_Loaded;
            InitializeComponent();
        }

        void BarsView_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = ServiceLocator.Current.GetInstance<BarsViewModel>();
            DataContext = viewModel;
        }

        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {

        }
    }
}
