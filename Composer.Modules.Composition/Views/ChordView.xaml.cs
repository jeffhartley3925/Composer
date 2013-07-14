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
using System.ComponentModel;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;
using System.Diagnostics;

namespace Composer.Modules.Composition.Views
{
    public partial class ChordView : UserControl, IChordView
    {
		private ChordViewModel viewModel = null;
        public string ChordId
        {
            get
            {
                return (string)GetValue(ChordIdProperty);
            }
            set
            {
                SetValue(ChordIdProperty, value);
                OnPropertyChanged("ChordId");
            }
        }

        public ChordView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ChordIdProperty =
            DependencyProperty.Register("ChordId", typeof(string), typeof(ChordView), new PropertyMetadata("", null));
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
				viewModel = new ChordViewModel(this.ChordId);
				_ViewModels.chords.Add(viewModel);
				this.DataContext = viewModel;
            }
        }
     }
}
