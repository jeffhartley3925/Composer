using System.Windows;
using Composer.Infrastructure;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Dimensions;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class BarsViewModel : BaseViewModel, IBarsViewModel
    {
        public BarsViewModel()
        {
            DefineCommands();
            SubscribeEvents();
        }

        public void DefineCommands()
        {
            BarSelectedCommand = new DelegateCommand<Bar>(OnBarSelectedCommand);
        }

        public void SubscribeEvents()
        {
        }

        private Visibility _captionVisibility = Visibility.Collapsed;
        public Visibility CaptionVisibility
        {
            get { return _captionVisibility; }
            set
            {
                if (value != _captionVisibility)
                {
                    _captionVisibility = value;
                    OnPropertyChanged(() => CaptionVisibility);
                }
            }
        }

        private string _captionText = string.Empty;
        public string CaptionText
        {
            get { return _captionText; }
            set
            {
                if (value != _captionText)
                {
                    _captionText = value;
                    OnPropertyChanged(() => CaptionText);
                }
            }
        }

        private List<Bar> _bars = Infrastructure.Dimensions.Bars.BarList;
        public List<Bar> Bars
        {
            get { return _bars; }
            set
            {
                if (value != _bars)
                {
                    _bars = value;
                    OnPropertyChanged(() => Bars);
                }
            }
        }

        private string _selectedBar;
        public string SelectedBar
        {
            get { return _selectedBar; }
            set
            {
                if (value != _selectedBar)
                {
                    _selectedBar = value;
                    OnPropertyChanged(() => SelectedBar);
                }
            }
        }

        public DelegateCommand<Bar> BarSelectedCommand { get; private set; }

        private void OnBarSelectedCommand(Bar bar)
        {
            Infrastructure.Dimensions.Bars.Bar = bar;
            CaptionText = string.Format("{0} Bar", bar.Name);
            CaptionVisibility = Visibility.Visible;
        }
    }
}
