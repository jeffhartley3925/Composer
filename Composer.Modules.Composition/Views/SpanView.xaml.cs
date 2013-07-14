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
    public partial class SpanView : UserControl, ISpanView
    {
        public string SpanId
        {
            get
            {
                return (string)GetValue(SpanIdProperty);
            }
            set
            {
                SetValue(SpanIdProperty, value);
                OnPropertyChanged("SpanId");
            }
        }

        public SpanView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SpanIdProperty =
            DependencyProperty.Register("SpanId", typeof(string), typeof(SpanView), new PropertyMetadata("", null));

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
                this.DataContext = new SpanViewModel(this.SpanId);
            }
        }
    }
}
