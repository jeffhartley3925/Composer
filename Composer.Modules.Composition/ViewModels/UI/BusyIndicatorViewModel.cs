using Composer.Infrastructure;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class BusyIndicatorViewModel : BaseViewModel, IBusyIndicatorViewModel
    {
        public BusyIndicatorViewModel()
        {
            HeaderText = "";
            BusyText = "Please wait...";
        }

        private string _busyText;

        public string BusyText
        {
            get { return _busyText; }
            set
            {
                if (value != _busyText)
                {
                    _busyText = value;
                    OnPropertyChanged(() => BusyText);
                }
            }
        }

        private string _headerText = string.Empty;

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                if (value != _headerText)
                {
                    _headerText = value;
                    OnPropertyChanged(() => HeaderText);
                }
            }
        }
    }
}