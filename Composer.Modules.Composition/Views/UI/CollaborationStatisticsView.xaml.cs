using Composer.Modules.Composition.ViewModels.UI;
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
    public partial class CollaborationStatisticsView : UserControl
    {
        private CollaborationStatisticsViewModel vm;

        public CollaborationStatisticsView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                vm = new CollaborationStatisticsViewModel();
                this.DataContext = vm;
            }
        }
    }
}
