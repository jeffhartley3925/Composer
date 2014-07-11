using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.ViewModels.Helpers;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.Views
{
    public partial class ChordView : UserControl, IChordView
    {
        private ChordViewModel viewModel = null;
        private static IEventAggregator ea;
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
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
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
                //ea.GetEvent<RespaceMeasureGroup>().Publish(new Tuple<Guid, int?>(viewModel.Chord.Measure_Id, null));
            }
        }
    }
}
