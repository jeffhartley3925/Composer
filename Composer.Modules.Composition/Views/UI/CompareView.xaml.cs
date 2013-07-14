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
