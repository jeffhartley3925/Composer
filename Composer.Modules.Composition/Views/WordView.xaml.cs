using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;

namespace Composer.Modules.Composition.Views
{
    public partial class WordView : Canvas, IWordView
    {
        WordViewModel ViewModel;

        public WordView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(WordView_Loaded);
        }

        public static readonly DependencyProperty MetaWordProperty = DependencyProperty.Register("MetaWord", typeof(string), typeof(WordView), new PropertyMetadata("", null));

        public string MetaWord
        {
            get { return (string)GetValue(MetaWordProperty); }
            set
            {
                SetValue(MetaWordProperty, value);
                OnPropertyChanged("MetaWord");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void WordView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.ViewModel = new WordViewModel(this.MetaWord);
                this.DataContext = this.ViewModel;
            }
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Width = double.NaN;
        }

    }
}