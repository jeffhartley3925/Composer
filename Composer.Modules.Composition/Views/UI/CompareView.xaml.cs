using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class CompareView : UserControl, ICompareView
    {
        public CompareView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(CompareView_Loaded);
        }

        void CompareView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.DataContext = new CompareViewModel();
            }
        }
    }
}
