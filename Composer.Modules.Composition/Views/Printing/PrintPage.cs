using System.Collections.ObjectModel;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Views
{
    public class PrintPage : BaseViewModel
    {
        private ObservableCollection<PrintPageItem> _printPageItems = new ObservableCollection<PrintPageItem>();
        public ObservableCollection<PrintPageItem> PrintPageItems
        {
            get 
            { 
                return _printPageItems; 
            }
            set
            {
                _printPageItems = value;
            }
        }

		private string _background = Preferences.PrintBackground;
		public string Background
		{
			get { return _background; }
			set { _background = value; }
		}

		private string _foreground = Preferences.PrintForeground;
		public string Foreground
		{
			get { return _foreground; }
			set { _foreground = value; }
		}
    }
}
