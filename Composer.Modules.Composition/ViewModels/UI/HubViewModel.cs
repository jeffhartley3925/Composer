using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition;
using Composer.Modules.Composition.EventArgs;
using Composer.Modules.Composition.Service;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Presentation.Commands;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Browser;

namespace Composer.Modules.Dialogs.ViewModels
{
    public sealed class HubViewModel : BaseViewModel, IHubViewModel
    {
        private bool _canExecuteEdit;
        private bool _canExecuteNew;
        private bool _compositionListEnabled;
        private bool _prevEnabled;
        private bool _nextEnabled = true;

        private Repository.DataService.Composition _selectedComposition;
        private CDataEntities _context;
        private CompositionService _playbackSservice;
        private HubCompositionsService _service;
        private ObservableCollection<Repository.DataService.Composition> _compositions;
        private ObservableCollection<Repository.DataService.Composition> _displayedCompositions;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpOnView;

        private string _lyricsLinkCaption = string.Empty;
        private string _nextText = "Next >";
        private string _prevText = "< Prev";
        private double _scrollHeight = Preferences.Hub.ScrollHeight;
        private int _displayedCompositionsOffset;

        private const int DisplayedCompositionCount = 4;

        private Visibility _nextVisibility = Visibility.Visible;
        private Visibility _pagingVisibility = Visibility.Collapsed;
        private Visibility _userIdVisibility = Visibility.Collapsed;
        private Visibility _usernameVisibility = Visibility.Collapsed;
        private Visibility _userPictureUrlVisibility = Visibility.Collapsed;
        private Visibility _prevVisibility = Visibility.Visible;

        public HubViewModel()
        {
            _compositionListEnabled = !EditorState.IsInternetAccess;
            GetHubCompositions();
            SubscribeEvents();
            DefineCommands();
            if (EditorState.IsFacebookDataLoaded)
            {
                CompositionListEnabled = true;
                CanExecuteNew = true;
            }
        }

        public bool CanExecuteEdit
        {
            get { return _canExecuteEdit; }
            set
            {
                _canExecuteEdit = value;
                EditClickCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanExecuteNew
        {
            get { return _canExecuteNew; }
            set
            {
                _canExecuteNew = value;
                if (NewClickCommand != null) NewClickCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegatedCommand<object> ClickNext { get; private set; }

        public DelegatedCommand<object> ClickPrev { get; private set; }

        public bool CompositionListEnabled
        {
            get { return _compositionListEnabled; }
            set
            {
                _compositionListEnabled = value;
                OnPropertyChanged(() => CompositionListEnabled);
            }
        }

        public ObservableCollection<Repository.DataService.Composition> Compositions
        {
            get { return _compositions; }
            set
            {
                _compositions = value;
                if (_compositions != null)
                {
                    if (EditorState.IsQueryStringSource())
                    {
                        EA.GetEvent<ForwardComposition>().Publish(_compositions[0].Id.ToString());
                        OnEditClick(string.Empty);
                        EA.GetEvent<SetCollaboratorIndex>().Publish(int.Parse(EditorState.qsIndex));
                    }
                    UpdateDisplayedCompositions();
                }
                OnPropertyChanged(() => Compositions);
            }
        }

        public CDataEntities Context
        {
            get { return _context ?? (_context = ServiceLocator.Current.GetInstance<CDataEntities>()); }
        }

        public ObservableCollection<Repository.DataService.Composition> DisplayedCompositions
        {
            get { return _displayedCompositions; }
            set
            {
                _displayedCompositions = value;
                DeleteLikeBtns();
                CreateLikeButtons();
                OnPropertyChanged(() => DisplayedCompositions);
            }
        }
        public DelegateCommand<object> EditClickCommand { get; private set; }

        public string LyricsLinkCaption
        {
            get { return _lyricsLinkCaption; }
            set
            {
                _lyricsLinkCaption = value;
                OnPropertyChanged(() => LyricsLinkCaption);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpOnView
        {
            get { return _mouseLeftButtonUpOnView; }
            set
            {
                _mouseLeftButtonUpOnView = value;
                OnPropertyChanged(() => MouseLeftButtonUpOnView);
            }
        }

        public DelegateCommand<object> NewClickCommand { get; private set; }

        public bool NextEnabled
        {
            get { return _nextEnabled; }
            set
            {
                _nextEnabled = value;
                OnPropertyChanged(() => NextEnabled);
            }
        }

        public string NextText
        {
            get { return _nextText; }
            set
            {
                _nextText = value;
                OnPropertyChanged(() => NextText);
            }
        }

        public Visibility NextVisibility
        {
            get { return _nextVisibility; }
            set
            {
                _nextVisibility = value;
                OnPropertyChanged(() => NextVisibility);
            }
        }

        public Visibility PagingVisibility
        {
            get { return _pagingVisibility; }
            set
            {
                _pagingVisibility = value;
                OnPropertyChanged(() => PagingVisibility);
            }
        }

        public bool PrevEnabled
        {
            get { return _prevEnabled; }
            set
            {
                _prevEnabled = value;
                OnPropertyChanged(() => PrevEnabled);
            }
        }

        public string PrevText
        {
            get { return _prevText; }
            set
            {
                _prevText = value;
                OnPropertyChanged(() => PrevText);
            }
        }

        public Visibility PrevVisibility
        {
            get { return _prevVisibility; }
            set
            {
                _prevVisibility = value;
                OnPropertyChanged(() => PrevVisibility);
            }
        }

        public double ScrollHeight
        {
            get { return _scrollHeight; }
            set
            {
                _scrollHeight = value;
                OnPropertyChanged(() => ScrollHeight);
            }
        }

        public Visibility UserIdVisibility
        {
            get { return _userIdVisibility; }
            set
            {
                _userIdVisibility = value;
                OnPropertyChanged(() => UserIdVisibility);
            }
        }

        public Visibility UsernameVisibility
        {
            get { return _usernameVisibility; }
            set
            {
                _usernameVisibility = value;
                OnPropertyChanged(() => UsernameVisibility);
            }
        }

        public Visibility UserPictureUrlVisibility
        {
            get { return _userPictureUrlVisibility; }
            set
            {
                _userPictureUrlVisibility = value;
                OnPropertyChanged(() => UserPictureUrlVisibility);
            }
        }

        private Guid CompositionId { get; set; }

        public bool OnCanExecuteEdit(object obj)
        {
            return CanExecuteEdit;
        }

        public bool OnCanExecuteNew(object obj)
        {
            return CanExecuteNew;
        }

        public void OnClickNext(object obj)
        {
            _displayedCompositionsOffset += DisplayedCompositionCount;
            UpdateDisplayedCompositions();
        }

        public void OnClickPrev(object obj)
        {
            _displayedCompositionsOffset -= DisplayedCompositionCount;
            UpdateDisplayedCompositions();
        }

        public void OnEditClick(object obj)
        {
            DeleteLikeBtns();
            EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
            var c = (from a in Compositions where a.Id == CompositionId && a.Audit.Author_Id == Current.User.Id select a);
            var compositions = c as List<Repository.DataService.Composition> ?? c.ToList();
            if (compositions.Any())
            {
                //selected composition authored by current logged in user
                EditorState.EditContext = _Enum.EditContext.Authoring;
                _selectedComposition = compositions.First();
            }
            else
            {
                var d = (from a in Compositions where a.Id == CompositionId && a.Audit.Author_Id != Current.User.Id select a);
                var e = d as List<Repository.DataService.Composition> ?? d.ToList();
                if (e.Any())
                {
                    //selected composition was authored by a friend of the current logged in user.
                    //ie: the currently logged in user is a contributor
                    EditorState.EditContext = _Enum.EditContext.Contributing;
                    _selectedComposition = e.First();
                }
            }

            if (_selectedComposition != null)
            {
                EA.GetEvent<LoadComposition>().Publish(_selectedComposition);
                EA.GetEvent<HideHub>().Publish(string.Empty);
            }
        }

        public void OnEditCompositionFromQueryString(object obj)
        {
            OnEditClick(string.Empty);
        }

        public void OnForwardComposition(string id)
        {
            CanExecuteEdit = false;
            Guid guid;
            if (Guid.TryParse(id, out guid))
            {
                CompositionId = guid;
                CanExecuteEdit = true;
            }
        }

        public void OnNewClick(object obj)
        {
            DeleteLikeBtns();
            EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
            CompositionManager.Initialize();
            MeasureManager.Initialize();
            EA.GetEvent<ShowNewCompositionPanel>().Publish(string.Empty);
            EA.GetEvent<HideHub>().Publish(string.Empty);
        }

        public void OnPlayCompositionFromHub(Guid compositionId)
        {
            Repository.DataService.Composition c = (from a in Compositions where a.Id == compositionId select a).Single();
            IUnityContainer container = Unity.Container;
            _playbackSservice = (CompositionService)container.Resolve<ICompositionService>();
            _playbackSservice.CompositionId = c.Id.ToString();
            _playbackSservice.CompositionLoadingComplete += GetCompositionForPlayComplete;
            _playbackSservice.CompositionLoadingError += GetCompositionForPlayError;
            _playbackSservice.GetCompositionAsync();
        }

        private void CreateLikeButtons()
        {
            HtmlPage.Window.Invoke("createLikeButtonContainers", DisplayedCompositions.Count);
            int index = 1;
            foreach (Repository.DataService.Composition c in DisplayedCompositions)
            {
                HtmlPage.Window.Invoke("createLikeButton", c.Id.ToString(), "0", index, index == DisplayedCompositions.Count);
                index++;
            }
        }

        private void DefineCommands()
        {
            EditClickCommand = new DelegateCommand<object>(OnEditClick, OnCanExecuteEdit);
            NewClickCommand = new DelegateCommand<object>(OnNewClick, OnCanExecuteNew);
            ClickNext = new DelegatedCommand<object>(OnClickNext);
            ClickPrev = new DelegatedCommand<object>(OnClickPrev);
            CanExecuteNew = !EditorState.IsInternetAccess;
            CanExecuteEdit = false;
        }

        private void DeleteLikeBtns()
        {
            HtmlPage.Window.Invoke("deleteLikeButtons", DisplayedCompositionCount);
        }

        private void GetCompositionForPlayComplete(object sender, CompositionLoadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var composition in e.Results)
                {
                    CompositionManager.Composition = composition;
                    Playback.Initialize();
                    EA.GetEvent<PlayComposition>().Publish(_Enum.PlaybackInitiatedFrom.Hub);
                    break;
                }
            });
        }

        private void GetCompositionForPlayError(object sender, CompositionErrorEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
            });
        }

        private void GetCompositionsForHubComplete(object sender, HubCompositionsLoadingEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var c = new ObservableCollection<Repository.DataService.Composition>();
                foreach (var composition in e.Results)
                {
                    c.Add(composition);
                }
                Compositions = c;
            });
        }

        private void GetCompositionsForHubError(object sender, HubCompositionsErrorEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
            });
        }

        private void GetHubCompositions()
        {
            if (EditorState.IsQueryStringSource())
            {
                Visible = Visibility.Collapsed;
            }

            var container = Unity.Container;
            _service = (HubCompositionsService)container.Resolve<IHubCompositionsService>();
            _service.HubCompositionsLoadingComplete += GetCompositionsForHubComplete;
            _service.HubCompositionsLoadingError += GetCompositionsForHubError;
            _service.GetHubCompositionsAsync();
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<CompositionLoaded>().Subscribe(Hide);
            EA.GetEvent<ShowHub>().Subscribe(Show);
            EA.GetEvent<HideHub>().Subscribe(Hide);
            EA.GetEvent<ForwardComposition>().Subscribe(OnForwardComposition);
            EA.GetEvent<PlayCompositionFromHub>().Subscribe(OnPlayCompositionFromHub);
        }

        private void UpdateDisplayedCompositions()
        {
            EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
            var d = Compositions.Skip(_displayedCompositionsOffset).Take(DisplayedCompositionCount);
            DisplayedCompositions = new ObservableCollection<Repository.DataService.Composition>(d.ToList());

            if (Compositions.Count > DisplayedCompositionCount)
            {
                PagingVisibility = Visibility.Visible;
                PrevEnabled = false;
                NextEnabled = false;
                if (_displayedCompositionsOffset == 0)
                {
                    NextEnabled = true;
                }
                else if (_displayedCompositionsOffset * DisplayedCompositionCount >= Compositions.Count)
                {
                    PrevEnabled = true;
                }
                else
                {
                    PrevEnabled = true;
                    NextEnabled = true;
                }
                NextVisibility = (NextEnabled ? Visibility.Visible : Visibility.Collapsed);
                PrevVisibility = (PrevEnabled ? Visibility.Visible : Visibility.Collapsed);
            }
        }
    }
}