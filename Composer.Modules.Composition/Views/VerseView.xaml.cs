using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.Models;
using Composer.Modules.Composition.ViewModels;
using System.Collections.ObjectModel;

namespace Composer.Modules.Composition.Views
{
    public partial class VerseView : UserControl, IVerseView
    {
        VerseViewModel ViewModel;

        public VerseView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(VerseView_Loaded);
        }

        public static readonly DependencyProperty VerseIndexProperty = DependencyProperty.Register("VerseIndex", typeof(string), typeof(VerseView), new PropertyMetadata("", null));

        public string VerseIndex
        {
            get { return (string)GetValue(VerseIndexProperty); }
            set
            {
                SetValue(VerseIndexProperty, value);
                OnPropertyChanged("VerseIndex");
            }
        }

        public static readonly DependencyProperty WordsProperty = DependencyProperty.Register("Words", typeof(ObservableCollection<Word>), typeof(VerseView), new PropertyMetadata(null, null));

        public ObservableCollection<Word> Words
        {
            get { return (ObservableCollection<Word>)GetValue(WordsProperty); }
            set
            {
                SetValue(WordsProperty, value);
                OnPropertyChanged("Words");
            }
        }

        public static readonly DependencyProperty DispositionProperty = DependencyProperty.Register("Disposition", typeof(int), typeof(VerseView), new PropertyMetadata(1, null));

        public int Disposition
        {
            get { return (int)GetValue(DispositionProperty); }
            set
            {
                SetValue(DispositionProperty, value);
                OnPropertyChanged("Disposition");
            }
        }

        public static readonly DependencyProperty MeasureIdProperty = DependencyProperty.Register("MeasureId", typeof(string), typeof(VerseView), new PropertyMetadata("", null));

        public string MeasureId
        {
            get { return (string)GetValue(MeasureIdProperty); }
            set
            {
                SetValue(MeasureIdProperty, value);
                OnPropertyChanged("MeasureId");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;

            if (ph != null)
                ph(this, new PropertyChangedEventArgs(name));
        }

        private void VerseView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.ViewModel = new VerseViewModel(this.VerseIndex, this.MeasureId, this.Words, (int)this.Disposition);
                this.DataContext = this.ViewModel;
            }
        }
    }
}