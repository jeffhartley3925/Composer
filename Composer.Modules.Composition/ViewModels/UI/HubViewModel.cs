using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Composer.Infrastructure;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.Service;
using Composer.Modules.Composition.EventArgs;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Windows.Browser;
using Composer.Infrastructure.Behavior;
using System.Windows.Controls;
using System.Windows.Input;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Support;
using System.Data.Services.Client;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Composer.Modules.Composition;
using System.Windows.Data;

namespace Composer.Modules.Dialogs.ViewModels
{
	public sealed class HubViewModel : BaseViewModel, IHubViewModel
	{
		private int displayedCompositionCount = 2;
		private int displayedCompositionsOffset = 0;
		private Repository.DataService.Composition _selectedComposition = null;
		private ObservableCollection<Repository.DataService.Composition> _displayedCompositions = null;
		public ObservableCollection<Repository.DataService.Composition> DisplayedCompositions
		{
			get { return _displayedCompositions; }
			set
			{
				_displayedCompositions = value;
				deleteLikeBtns();
				CreateLikeButtons();
				OnPropertyChanged(() => DisplayedCompositions);
			}
		}

		private ObservableCollection<Repository.DataService.Composition> _compositions = null;
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

		private void UpdateDisplayedCompositions()
		{
			EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
			var d = Compositions.Skip(displayedCompositionsOffset).Take(displayedCompositionCount);
			DisplayedCompositions = new ObservableCollection<Repository.DataService.Composition>(d.ToList());

			if (Compositions.Count > displayedCompositionCount)
			{
				PagingVisibility = Visibility.Visible;
				PrevEnabled = false;
				NextEnabled = false;
				if (displayedCompositionsOffset == 0)
				{
					NextEnabled = true;
				}
				else if (displayedCompositionsOffset * displayedCompositionCount >= Compositions.Count)
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

		private void deleteLikeBtns()
		{
			HtmlPage.Window.Invoke("deleteLikeButtons", displayedCompositionCount);
		}

		private double _scrollHeight = Preferences.Hub.ScrollHeight;
		public double ScrollHeight
		{
			get { return _scrollHeight; }
			set
			{
				_scrollHeight = value;
				OnPropertyChanged(() => ScrollHeight);
			}
		}

		private void GetHubCompositions()
		{
			string id = string.Empty;
			if (EditorState.IsQueryStringSource())
			{
				id = EditorState.qsId.ToString();
				Visible = Visibility.Collapsed;
			}
			var dataQuery = (DataServiceQuery<Repository.DataService.Composition>)this.Context
				.CreateQuery<Repository.DataService.Composition>("HubCompositions")
				.AddQueryOption("Audit_Author_Id", string.Format("'{0}'", Current.User.Id))
				.AddQueryOption("FriendIds", string.Format("'{0}'", FacebookData.FriendIds.Aggregate((x, y) => x + "," + y)))
				.AddQueryOption("Id", string.Format("'{0}'", id));

			dataQuery.BeginExecute(result =>
			{
				Compositions = new DataServiceCollection<Repository.DataService.Composition>(
					dataQuery.EndExecute(result));

			}, null);

		}

		public HubViewModel()
		{
			GetHubCompositions();
			SubscribeEvents();
			DefineCommands();
			if (EditorState.IsFacebookDataLoaded)
			{
				CompositionListEnabled = true;
				CanExecuteNew = true;
			}
		}

		private void DefineCommands()
		{
			EditClickCommand = new DelegateCommand<object>(OnEditClick, OnCanExecuteEdit);
			NewClickCommand = new DelegateCommand<object>(OnNewClick, OnCanExecuteNew);
			ClickNext = new DelegatedCommand<object>(OnClickNext);
			ClickPrev = new DelegatedCommand<object>(OnClickPrev);
			CanExecuteNew = false || !EditorState.IsInternetAccess;
			CanExecuteEdit = false;
		}

		private void SubscribeEvents()
		{
			EA.GetEvent<CompositionLoaded>().Subscribe(Hide);
			EA.GetEvent<ShowHub>().Subscribe(Show);
			EA.GetEvent<HideHub>().Subscribe(Hide);
			EA.GetEvent<ForwardComposition>().Subscribe(OnForwardComposition);
			EA.GetEvent<PlayCompositionFromHub>().Subscribe(OnPlayCompositionFromHub);
		}

		public void OnPlayCompositionFromHub(Guid compositionId)
		{
			Repository.DataService.Composition c = (from a in Compositions where a.Id == compositionId select a).Single();

			IUnityContainer container = Unity.Container;
			_service = (CompositionService)container.Resolve<ICompositionService>();
			_service.CompositionId = c.Id.ToString();
			_service.CompositionLoadingComplete += CompositionLoadingComplete;
			_service.CompositionLoadingError += CompositionLoadingError;
			_service.GetCompositionAsync();

		}
		private void CompositionLoadingError(object sender, CompositionErrorEventArgs e)
		{
			Deployment.Current.Dispatcher.BeginInvoke(() =>
			{

			});
		}

		private void CompositionLoadingComplete(object sender, CompositionLoadingEventArgs e)
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
		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpOnView;
		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpOnView
		{
			get { return _mouseLeftButtonUpOnView; }
			set
			{
				_mouseLeftButtonUpOnView = value;
				OnPropertyChanged(() => MouseLeftButtonUpOnView);
			}
		}

		private Visibility _usernameVisibility = Visibility.Collapsed;
		public Visibility UsernameVisibility
		{
			get { return _usernameVisibility; }
			set
			{
				_usernameVisibility = value;
				OnPropertyChanged(() => UsernameVisibility);
			}
		}


		private Visibility _userIdVisibility = Visibility.Collapsed;
		public Visibility UserIdVisibility
		{
			get { return _userIdVisibility; }
			set
			{
				_userIdVisibility = value;
				OnPropertyChanged(() => UserIdVisibility);
			}
		}


		private Visibility _userPictureUrlVisibility = Visibility.Collapsed;
		public Visibility UserPictureUrlVisibility
		{
			get { return _userPictureUrlVisibility; }
			set
			{
				_userPictureUrlVisibility = value;
				OnPropertyChanged(() => UserPictureUrlVisibility);
			}
		}

		private bool _compositionListEnabled = false || !EditorState.IsInternetAccess;
		public bool CompositionListEnabled
		{
			get { return _compositionListEnabled; }
			set
			{
				_compositionListEnabled = value;
				OnPropertyChanged(() => CompositionListEnabled);
			}
		}

		private bool _canExecuteEdit;
		public bool CanExecuteEdit
		{
			get { return _canExecuteEdit; }
			set
			{
				_canExecuteEdit = value;
				EditClickCommand.RaiseCanExecuteChanged();
			}
		}

		public bool OnCanExecuteEdit(object obj)
		{
			return CanExecuteEdit;
		}

		public DelegateCommand<object> EditClickCommand { get; private set; }

		private Guid CompositionId { get; set; }

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

		public void OnEditClick(object obj)
		{
			deleteLikeBtns();
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
		private CompositionService _service;

		public DelegateCommand<object> NewClickCommand { get; private set; }

		public void OnNewClick(object obj)
		{
			deleteLikeBtns();
			EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
			CompositionManager.Initialize();
			MeasureManager.Initialize();
			EA.GetEvent<ShowNewCompositionPanel>().Publish(string.Empty);
			EA.GetEvent<HideHub>().Publish(string.Empty);
		}

		private bool _canExecuteNew;
		public bool CanExecuteNew
		{
			get { return _canExecuteNew; }
			set
			{
				_canExecuteNew = value;
				if (NewClickCommand != null) NewClickCommand.RaiseCanExecuteChanged();
			}
		}

		public bool OnCanExecuteNew(object obj)
		{
			return CanExecuteNew;
		}

		public DelegatedCommand<object> ClickNext { get; private set; }

		public void OnClickNext(object obj)
		{
			displayedCompositionsOffset += displayedCompositionCount;
			UpdateDisplayedCompositions();
		}

		public DelegatedCommand<object> ClickPrev { get; private set; }

		public void OnClickPrev(object obj)
		{
			displayedCompositionsOffset -= displayedCompositionCount;
			UpdateDisplayedCompositions();
		}

		private CDataEntities context;
		public CDataEntities Context
		{
			get
			{
				if (context == null)
				{
					context = ServiceLocator.Current.GetInstance<CDataEntities>();
				}
				return context;
			}
		}

		private bool _nextEnabled = true;
		public bool NextEnabled
		{
			get { return _nextEnabled; }
			set
			{
				_nextEnabled = value;
				OnPropertyChanged(() => NextEnabled);
			}
		}

		private bool _prevEnabled = false;
		public bool PrevEnabled
		{
			get { return _prevEnabled; }
			set
			{
				_prevEnabled = value;
				OnPropertyChanged(() => PrevEnabled);
			}
		}

		private string _prevText = "< Prev";
		public string PrevText
		{
			get { return _prevText; }
			set
			{
				_prevText = value;
				OnPropertyChanged(() => PrevText);
			}
		}

		private string _nextText = "Next >";
		public string NextText
		{
			get { return _nextText; }
			set
			{
				_nextText = value;
				OnPropertyChanged(() => NextText);
			}
		}

		private Visibility _nextVisibility = Visibility.Visible;
		public Visibility NextVisibility
		{
			get { return _nextVisibility; }
			set
			{
				_nextVisibility = value;
				OnPropertyChanged(() => NextVisibility);
			}
		}

		private Visibility _prevVisibility = Visibility.Visible;
		public Visibility PrevVisibility
		{
			get { return _prevVisibility; }
			set
			{
				_prevVisibility = value;
				OnPropertyChanged(() => PrevVisibility);
			}
		}
		private Visibility _pagingVisibility = Visibility.Collapsed;
		public Visibility PagingVisibility
		{
			get { return _pagingVisibility; }
			set
			{
				_pagingVisibility = value;
				OnPropertyChanged(() => PagingVisibility);
			}
		}
	}
}
