using Composer.Infrastructure;
using System.Windows;

namespace Composer.Modules.Composition.ViewModels
{
    public class Page : BaseViewModel
    {
        private int _number;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                OnPropertyChanged(() => Number);
            }
        }


        private string _foreground = "Black";
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Foreground);
            }
        }

        private Visibility _pageVisibility;
        public Visibility PageVisibility
        {
            get { return _pageVisibility; }
            set
            {
                _pageVisibility = value;
                OnPropertyChanged(() => PageVisibility);
            }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged(() => Content);
            }
        }

        public Page(int number, Visibility visibility)
        {
            Number = number;
            PageVisibility = visibility;
        }
    }
}
