using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Palettes.ViewModels;

namespace Composer.Modules.Palettes.Views
{
    public partial class PaletteRadioButtonView : UserControl, IPaletteButtonView
    {
        public PaletteRadioButtonView()
        {
            InitializeComponent();
        }

        public string Target
        {
            get
            {
                return (string)GetValue(TargetProperty);
            }
            set
            {
                SetValue(TargetProperty, value);
                OnPropertyChanged("Target");
            }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

        public string GroupName
        {
            get
            {
                return (string)GetValue(GroupNameProperty);
            }
            set
            {
                SetValue(GroupNameProperty, value);
                OnPropertyChanged("GroupName");
            }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

        public string PaletteId
        {
            get
            {
                return (string)GetValue(PaletteIdProperty);
            }
            set
            {
                SetValue(PaletteIdProperty, value);
                OnPropertyChanged("PaletteId");
            }
        }

        public static readonly DependencyProperty PaletteIdProperty =
            DependencyProperty.Register("PaletteId", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

        public string Enabled
        {
            get
            {
                return (string)GetValue(EnabledProperty);
            }
            set
            {
                SetValue(EnabledProperty, value);
                OnPropertyChanged("Enabled");
            }
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

        public string Caption
        {
            get
            {
                return (string)GetValue(CaptionProperty);
            }
            set
            {
                SetValue(CaptionProperty, value);
                OnPropertyChanged("Caption");
            }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

        public string Tooltip
        {
            get
            {
                return (string)GetValue(TooltipProperty);
            }
            set
            {
                SetValue(TooltipProperty, value);
                OnPropertyChanged("Tooltip");
            }
        }

        public static readonly DependencyProperty TooltipProperty =
            DependencyProperty.Register("Tooltip", typeof(string), typeof(PaletteRadioButtonView), new PropertyMetadata("", null));

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
                this.DataContext = new PaletteButtonViewModel(this.Enabled, this.Target, this.GroupName, this.Caption, this.Tooltip, this.PaletteId);
            }
        }
    }
}