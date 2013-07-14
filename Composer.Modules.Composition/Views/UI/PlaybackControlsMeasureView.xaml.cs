using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class PlaybackControlsMeasureView : UserControl, IPlaybackControlsView
    {
        private PlaybackControlsViewModel vm;

        public PlaybackControlsMeasureView()
        {
            InitializeComponent();
        }

        public string TargetId
        {
            get { return (string)GetValue(TargetIdProperty); }
            set
            {
                if ((string)GetValue(TargetIdProperty) != value)
                {
                    SetValue(TargetIdProperty, value);
                    OnPropertyChanged("TargetId");
                }
            }
        }

        public static readonly DependencyProperty TargetIdProperty =
            DependencyProperty.Register("TargetId", typeof(string), typeof(PlaybackControlsMeasureView), new PropertyMetadata("", null));

        public string Location
        {
            get { return (string)GetValue(LocationProperty); }
            set
            {
                if ((string)GetValue(LocationProperty) != value)
                {
                    SetValue(LocationProperty, value);
                    OnPropertyChanged("Location");
                }
            }
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(string), typeof(PlaybackControlsMeasureView), new PropertyMetadata("", null));

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
                //if (!string.IsNullOrEmpty(this.TargetId))
                //{
                    vm = new PlaybackControlsViewModel(this.TargetId, this.Location);
                    this.DataContext = vm;
                //}
            }
        }
    }
}
