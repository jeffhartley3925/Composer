using Composer.Infrastructure;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Modules.Composition.ViewModels
{
    public class Notification : BaseViewModel
    {
        private string _lastChangeDate;
        public string LastChangeDate
        {
            get { return _lastChangeDate; }
            set
            {
                _lastChangeDate = value;
                OnPropertyChanged(() => LastChangeDate);
            }
        }

        private string _collaboratorId = string.Empty;
        public string CollaboratorId
        {
            get { return _collaboratorId; }
            set
            {
                _collaboratorId = value;
                OnPropertyChanged(() => CollaboratorId);
            }
        }


        private int _collaboratorIndex = 0;
        public int CollaboratorIndex
        {
            get { return _collaboratorIndex; }
            set
            {
                _collaboratorIndex = value;
                OnPropertyChanged(() => CollaboratorIndex);
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

        private string _collaboratorImage;
        public string CollaboratorImage
        {
            get { return _collaboratorImage; }
            set
            {
                _collaboratorImage = value;
                OnPropertyChanged(() => CollaboratorImage);
            }
        }

        private string _collaboratorName;
        public string CollaboratorName
        {
            get { return _collaboratorName; }
            set
            {
                _collaboratorName = value;
                OnPropertyChanged(() => CollaboratorName);
            }
        }

        private string _pictureUrl;
        public string PictureUrl
        {
            get { return _pictureUrl; }
            set
            {
                _pictureUrl = value;
                OnPropertyChanged(() => PictureUrl);
            }
        }

        private string _lyricsAdded = string.Empty;
        public string LyricsAdded
        {
            get { return _lyricsAdded; }
            set
            {
                _lyricsAdded = value;
                OnPropertyChanged(() => LyricsAdded);
            }
        }

        private string _suggestionsToYou = string.Empty;
        public string SuggestionsToYou
        {
            get { return _suggestionsToYou; }
            set
            {
                _suggestionsToYou = value;
                OnPropertyChanged(() => SuggestionsToYou);
            }
        }

        private string _yourChangesAccepted = string.Empty;
        public string YourChangesAccepted
        {
            get { return _yourChangesAccepted; }
            set
            {
                _yourChangesAccepted = value;
                OnPropertyChanged(() => YourChangesAccepted);
            }
        }

        private string _yourChangesRejected = string.Empty;
        public string YourChangesRejected
        {
            get { return _yourChangesRejected; }
            set
            {
                _yourChangesRejected = value;
                OnPropertyChanged(() => YourChangesRejected);
            }
        }

        public Notification()
        {
        }

        public Notification(string name, int index, string id)
        {
            CollaboratorName = name;
            CollaboratorId = id;
            CollaboratorIndex = index;
        }

        public Notification(Composer.Repository.DataService.Collaboration c)
        {
            CollaboratorName = c.Name;
            CollaboratorId = c.Collaborator_Id;
            CollaboratorIndex = c.Index;
            LastChangeDate = ((DateTime)c.LastChangeDate).ToShortDateString();
            PictureUrl = c.PictureUrl;
        }
    }
}
