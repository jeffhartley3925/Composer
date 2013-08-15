using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;
using System;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Windows.Browser;
using Liquid;
using Composer.Infrastructure.Constants;

namespace Composer.Silverlight.UI
{
	public sealed class ShellViewModel : BaseViewModel
	{
        private string _selectedCompositionImageUri = string.Empty;
		public ShellViewModel()
		{

			DefineCommands();
			SubscribeEvents();

            LyricsPanelVisibility = Visibility.Visible;
            LyricsPanel_X = -600;
            LyricsPanel_Y = -600;

            SavePanelVisibility = Visibility.Visible;
            SavePanel_X = -600;
            SavePanel_Y = -600;

            TransposePanelVisibility = Visibility.Visible;
            TransposePanel_X = -600;
            TransposePanel_Y = -600;

            //DELETE: BarView is not in use.
            BarsVisibility = Visibility.Collapsed;
            Bars_X = 300;
            Bars_Y = 100;

            NoteEditorVisibility = Visibility.Collapsed;
            NoteEditor_X = -600;
            NoteEditor_Y = -600;

            NewCompositionPanelVisibility = Visibility.Collapsed;
            NewCompositionPanel_X = 300;
            NewCompositionPanel_Y = 100;

			EditPopupVisibility = Visibility.Visible;
			PrintingVisibility = Visibility.Collapsed;
			CompositionVisibility = Visibility.Visible;

            SidebarVisibility = Visibility.Collapsed;

			BusyIndicator_X = 330;
			BusyIndicator_Y = 150;
			BusyIndicatorVisibility = Visibility.Collapsed;

			ExceptionMessage_X = 300;
			ExceptionMessage_Y = 140;
			ExceptionMessageVisibility = Visibility.Collapsed;

			CollaborationsVisibility = Visibility.Collapsed;
			EditorState.Collaborating = false;

			Collaborations_X = 450;
			Collaborations_Y = 280;
		}

        private Guid _compositionId = Guid.Empty;
        public Guid Id
        {
            get { return _compositionId; }
            set
            {
                _compositionId = value;
            }
        }

		private void DefineCommands()
		{
			MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, CanReactToMouseMove);
			MouseMoveSidebarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMoveSidebar, CanReactToMouseMoveSidebar);
			MouseLeftButtonUpCommand = new DelegatedCommand<object>(OnMouseLeftButtonUpCommand, CanReactToMouseClick);
			LogExceptionLeftButtonUpCommand = new DelegatedCommand<object>(OnLogException, CanLogExceptions);
			CloseExceptionLeftButtonUpCommand = new DelegatedCommand<object>(OnCloseException);
			PrintCommand = new DelegatedCommand<object>(OnPrintCommand);
		}

		public void OnSetEditPopup(Tuple<Point, int, int, double, double, string, Guid> payload)
		{
            Point pt = Utilities.CoordinateSystem.TranslateToCompositionCoords(payload.Item1.X, payload.Item1.Y, payload.Item2, payload.Item3, payload.Item4, payload.Item5, payload.Item6, payload.Item7);
			EditPopup_X = pt.X;
			EditPopup_Y = pt.Y;
		}

		public void OnPrintCommand(object obj)
		{
			CompositionManager.HideSocialChannels();
			EA.GetEvent<SetPrint>().Publish(string.Empty);
		}

		private bool CanLogExceptions(object obj)
		{
			return false;
		}

		private bool CanReactToMouseClick(object obj)
		{
			return EditorState.IsComposing;
		}

		private bool CanReactToMouseMove(object obj)
		{
			return EditorState.IsComposing;
		}

		private bool CanReactToMouseMoveSidebar(object obj)
		{
            return false;
		}

		private void SubscribeEvents()
		{
            EA.GetEvent<HubCollaboratorMouseEnter>().Subscribe(OnHubCollaboratorMouseEnter);
            EA.GetEvent<HubCollaboratorMouseLeave>().Subscribe(OnHubCollaboratorMouseLeave);
            EA.GetEvent<HubCollaboratorMouseClick>().Subscribe(OnHubCollaboratorMouseClick);
            EA.GetEvent<HubCompositionMouseEnter>().Subscribe(OnHubCompositionMouseEnter);
            EA.GetEvent<HubCompositionMouseLeave>().Subscribe(OnHubCompositionMouseLeave);
            EA.GetEvent<HubCompositionMouseClick>().Subscribe(OnHubCompositionMouseClick);
            EA.GetEvent<UpdateCompositionImage>().Subscribe(OnUpdateCompositionImage);
            EA.GetEvent<FacebookDataLoaded>().Subscribe(OnFacebookDataLoaded);
            EA.GetEvent<LoadComposition>().Subscribe(OnLoadComposition);
            EA.GetEvent<HideNoteEditor>().Subscribe(OnHideNoteEditor, true);
            EA.GetEvent<ShowNoteEditor>().Subscribe(OnShowNoteEditor, true);
            EA.GetEvent<ShowLyricsPanel>().Subscribe(OnShowLyricsPanel, true);
            EA.GetEvent<HideLyricsPanel>().Subscribe(OnHideLyricsPanel, true);
            EA.GetEvent<ShowSavePanel>().Subscribe(OnShowSavePanel, true);
            EA.GetEvent<HideSavePanel>().Subscribe(OnHideSavePanel, true);
            EA.GetEvent<ShowTransposePanel>().Subscribe(OnShowTransposePanel, true);
            EA.GetEvent<HideTransposePanel>().Subscribe(OnHideTransposePanel, true);
			EA.GetEvent<ResizeViewport>().Subscribe(OnResizeViewPort);
			EA.GetEvent<ToolPaletteClicked>().Subscribe(OnToolSelected, true);
			EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
			EA.GetEvent<SuspendEditing>().Subscribe(OnSuspendEditing);
			EA.GetEvent<ShowBusyIndicator>().Subscribe(OnShowBusyIndicator);
			EA.GetEvent<HideBusyIndicator>().Subscribe(OnHideBusyIndicator);
			EA.GetEvent<ScaleViewportChanged>().Subscribe(OnUiScaleChanged, true);
			EA.GetEvent<UpdateCollaboratorName>().Subscribe(OnUpdateCollaboratorName);
			EA.GetEvent<HideCollaborationPanel>().Subscribe(OnHideCollaborationPanel);
			EA.GetEvent<ShowCollaborationPanel>().Subscribe(OnShowCollaborationPanel);
			EA.GetEvent<DisplayExceptionMessage>().Subscribe(OnDisplayExceptionMessage);
            EA.GetEvent<ToggleSidebarVisibility>().Subscribe(OnToggleSidebarVisibility);
			EA.GetEvent<PlaceCompositionPanel>().Subscribe(OnPlaceCompositionPanel);
			EA.GetEvent<SetPrint>().Subscribe(OnSetPrint);
			EA.GetEvent<ClosePrintPreview>().Subscribe(OnRemovePrint);
			EA.GetEvent<SetEditPopupMenu>().Subscribe(OnSetEditPopup);
			EA.GetEvent<ShowEditPopupMenu>().Subscribe(OnShowEditPopup);
			EA.GetEvent<HideEditPopup>().Subscribe(OnHideEditPopup);
            EA.GetEvent<ShowNewCompositionPanel>().Subscribe(OnShowNewCompositionPanel);
            EA.GetEvent<HideNewCompositionPanel>().Subscribe(OnHideNewCompositionPanel);
            EA.GetEvent<CheckFacebookDataLoaded>().Subscribe(OnCheckFacebookDataLoaded);
            EA.GetEvent<DisplayMessage>().Subscribe(OnDisplayMessage);
		}

        public void OnHubCollaboratorMouseEnter(string source)
        {
            EA.GetEvent<UpdateCompositionImage>().Publish(source);
            CompositionImageVisibility = Visibility.Visible;
        }

        public void OnHubCollaboratorMouseLeave(string source)
        {
            EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
            CompositionImageVisibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(_selectedCompositionImageUri))
            {
                OnHubCollaboratorMouseEnter(_selectedCompositionImageUri);
            }
        }

        public void OnHubCollaboratorMouseClick(string source)
        {
            if (source != _selectedCompositionImageUri)
            {
                EA.GetEvent<UpdateCompositionImage>().Publish(source);
                CompositionImageVisibility = Visibility.Visible;
                _selectedCompositionImageUri = source;
            }
            else
            {
                CompositionImageVisibility = Visibility.Collapsed;
                EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
                _selectedCompositionImageUri = string.Empty;
            }

        }

        public void OnHubCompositionMouseEnter(string source)
        {
            EA.GetEvent<UpdateCompositionImage>().Publish(source);
            CompositionImageVisibility = Visibility.Visible;
        }

        public void OnHubCompositionMouseLeave(string source)
        {
            EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
            CompositionImageVisibility = Visibility.Collapsed;
            if (! string.IsNullOrEmpty(_selectedCompositionImageUri))
            {
                OnHubCompositionMouseEnter(_selectedCompositionImageUri);
            }
        }

        public void OnHubCompositionMouseClick(string source)
        {
            if (source != _selectedCompositionImageUri)
            {
                EA.GetEvent<UpdateCompositionImage>().Publish(source);
                CompositionImageVisibility = Visibility.Visible;
                _selectedCompositionImageUri = source;
            }
            else
            {
                CompositionImageVisibility = Visibility.Collapsed;
                EA.GetEvent<UpdateCompositionImage>().Publish(string.Empty);
                _selectedCompositionImageUri = string.Empty;
            }

        }

        public void OnUpdateCompositionImage(string source)
        {
            this.CompositionImage = source;
            if (source == string.Empty)
            {
                this.CompositionImageVisibility = Visibility.Collapsed;
            }
        }

        public void OnDisplayMessage(string message)
        {
            this.Message = message;
        }

        public bool bLoaded = false;

        public void OnCheckFacebookDataLoaded(object obj)
        {
            Tuple<string, string, string, string, string, string> payload;
            FacebookData.Initialize();
            if (EditorState.IsInternetAccess)
            {
                var doc = HtmlPage.Document;

                var useridElement = doc.GetElementById("uid");
                var usernameElement = doc.GetElementById("username");
                var urlElement = doc.GetElementById("userimageurl");
                var friendsElement = doc.GetElementById("friends");
                var namesElement = doc.GetElementById("names");
                var picturesElement = doc.GetElementById("pictures");

                var username = (usernameElement != null) ? usernameElement.GetProperty("value").ToString() : string.Empty;
                var uid = (useridElement != null) ? useridElement.GetProperty("value").ToString() : string.Empty;
                var userurl = (urlElement != null) ? urlElement.GetProperty("value").ToString() : string.Empty;
                var friends = (friendsElement != null) ? friendsElement.GetProperty("value").ToString() : string.Empty;
                var names = (namesElement != null) ? namesElement.GetProperty("value").ToString() : string.Empty;
                var pictures = (picturesElement != null) ? picturesElement.GetProperty("value").ToString() : string.Empty;

                if (pictures.Length > 0 && names.Length > 0 && friends.Length > 0 && uid.Length > 0 && username.Length > 0 && userurl.Length > 0 && !bLoaded)
                {
                    bLoaded = true;
                    Current.User.Name = username;
                    Current.User.Id = uid;
                    Current.User.PictureUrl = (userurl.ToLower().IndexOf(".gif", StringComparison.Ordinal) > 0) ? Defaults.DefaultImageUrl : userurl;

                    payload = new Tuple<string, string, string, string, string, string>(uid, userurl, username, friends, names, pictures);
                    EA.GetEvent<FacebookDataLoaded>().Publish(payload);

                    if (!EditorState.IsLoggedIn)
                    {
                        EA.GetEvent<Login>().Publish(string.Empty);
                    }
                }
            }
            else
            {
                if (EditorState.IdIdToUse == 1)
                {
                    Current.User.Name = "Jeffrey W Hartley";
                    Current.User.PictureUrl = "http://fbcdn-profile-w.akamaihd.net/hprofile-ak-prn1/48583_675485908_4365_q.jpg";
                    Current.User.Id = "675485908";
                }
                else
                {
                    Current.User.Name = "John Smith";
                    Current.User.PictureUrl = "https://fbcdn-profile-w.akamaihd.net/hprofile-ak-ash4/275939_1421255088_1815427294_q.jpg";
                    Current.User.Id = "100004074923652";
                }
                payload = new Tuple<string, string, string, string, string, string>(Current.User.Id, Current.User.Name, Current.User.PictureUrl, "", "", "");
                EA.GetEvent<FacebookDataLoaded>().Publish(payload);
                if (!EditorState.IsLoggedIn)
                {
                    EA.GetEvent<Login>().Publish(string.Empty);
                }
            }
        }

        public void OnFacebookDataLoaded(object obj)
        {
            EditorState.IsFacebookDataLoaded = true;
        }

        public void OnLoadComposition(object obj)
        {
            var composition = (Repository.DataService.Composition)obj;
            this.Id = composition.Id;
        }

        public void OnHideNoteEditor(object obj)
        {
            NoteEditorVisibility = Visibility.Collapsed;
            EditorState.IsNoteEditor = false;
            NoteEditor_X = 600;
            NoteEditor_Y = 10;
        }

        public void OnShowNoteEditor(object obj)
        {
            NoteEditor_X = 300;
            NoteEditor_Y = 100;
            EditorState.IsNoteEditor = true;
            NoteEditorVisibility = Visibility.Visible;
        }

        public void OnShowNewCompositionPanel(object obj)
        {
            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            NewCompositionPanelVisibility = Visibility.Visible;
        }

        public void OnHideNewCompositionPanel(object obj)
        {
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
            NewCompositionPanelVisibility = Visibility.Collapsed;
        }

        public void OnShowTransposePanel(object obj)
        {
            //allow user to see results of transposition without dismissing the transposition panel.
            EditorState.BlurRadius = 0;

            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            EditorState.IsTransposing = true;
            TransposePanelVisibility = Visibility.Visible;
            EA.GetEvent<AnimateViewBorder>().Publish("Transpose Panel");
            TransposePanel_X = 600;
            TransposePanel_Y = 10;
            //reset
            EditorState.BlurRadius = Preferences.DefaultBlurRadius;
        }

        public void OnHideTransposePanel(object obj)
        {
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
            EditorState.IsTransposing = false;
            TransposePanel_X = -600;
            TransposePanel_Y = -600;
            TransposePanelVisibility = Visibility.Collapsed;
        }

        public void OnShowLyricsPanel(object obj)
        {
            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            EditorState.IsEditingLyrics = true;
            LyricsPanel_X = 300;
            LyricsPanel_Y = 100;
            LyricsPanelVisibility = Visibility.Visible;
            EA.GetEvent<AnimateViewBorder>().Publish("Lyrics Panel");
        }

        public void OnHideLyricsPanel(object obj)
        {
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
            EditorState.IsEditingLyrics = false;
            LyricsPanel_X = -600;
            LyricsPanel_Y = -600;
            LyricsPanelVisibility = Visibility.Collapsed;
        }

        public void OnShowSavePanel(object obj)
        {
            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            SavePanel_X = 300;
            SavePanel_Y = 100;
            SavePanelVisibility = Visibility.Visible;
            EA.GetEvent<AnimateViewBorder>().Publish("Save Panel");
            EA.GetEvent<ToggleHyperlinkVisibility>().Publish(new Tuple<Visibility, _Enum.HyperlinkButton>(Visibility.Collapsed, _Enum.HyperlinkButton.All));
        }

        public void OnHideSavePanel(object obj)
        {
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
            SavePanel_X = -600;
            SavePanel_Y = -600;
            SavePanelVisibility = Visibility.Collapsed;
        }

		public void OnShowEditPopup(object obj)
		{
			EditPopupVisibility = Visibility.Visible;
		}

		public void OnHideEditPopup(object obj)
		{
			EditPopupVisibility = Visibility.Collapsed;
		}

		public void OnRemovePrint(object obj)
		{
			CompositionVisibility = Visibility.Visible;
			PrintingVisibility = Visibility.Collapsed;
            EA.GetEvent<ScaleViewportChanged>().Publish(1);
            HtmlPage.Window.Invoke("removePrint", null);
            EditorState.IsPrinting = false;
		}

		public void OnSetPrint(object obj)
		{
            EditorState.IsPrinting = true;
			HtmlDocument htmlDoc = HtmlPage.Document;
			HtmlElement htmlEl = htmlDoc.GetElementById("plugin");
			if (htmlEl == null)
                throw new Exception("ShellViewModel - OnSetPrint");
			else
			{
                HtmlPage.Window.Invoke("setPrint", CompositionManager.Composition.Staffgroups.Count().ToString(CultureInfo.InvariantCulture));
                CompositionVisibility = Visibility.Collapsed;
                PrintingVisibility = Visibility.Visible;
			}
		}

        private string _compositionImage = string.Empty;
        public string CompositionImage
        {
            get { return _compositionImage; }
            set
            {
                _compositionImage = value;
                OnPropertyChanged(() => CompositionImage);
            }
        }

        private string _compositionImageMargin = Preferences.Hub.CompositionImage.Margin;
        public string CompositionImageMargin
        {
            get { return _compositionImageMargin; }
            set
            {
                _compositionImageMargin = value;
                OnPropertyChanged(() => CompositionImageMargin);
            }
        }

        private string _compositionImageBorderColor = Preferences.Hub.CompositionImage.BorderColor;
        public string CompositionImageBorderColor
        {
            get { return _compositionImageBorderColor; }
            set
            {
                _compositionImageBorderColor = value;
                OnPropertyChanged(() => CompositionImageBorderColor);
            }
        }

        private double _compositionImageBorderWidth = Preferences.Hub.CompositionImage.BorderWidth;
        public double CompositionImageBorderWidth
        {
            get { return _compositionImageBorderWidth; }
            set
            {
                _compositionImageBorderWidth = value;
                OnPropertyChanged(() => CompositionImageBorderWidth);
            }
        }

        private double _compositionImageScale = Preferences.Hub.CompositionImage.Scale;
        public double CompositionImageScale
        {
            get { return _compositionImageScale; }
            set
            {
                _compositionImageScale = value;
                OnPropertyChanged(() => CompositionImageScale);
            }
        }

        private double _compositionImageWidth = Preferences.Hub.CompositionImage.Width;
        public double CompositionImageWidth
        {
            get { return _compositionImageWidth; }
            set
            {
                _compositionImageWidth = value;
                OnPropertyChanged(() => CompositionImageWidth);
            }
        }

        private double _compositionImageHeight = Preferences.Hub.CompositionImage.Height;
        public double CompositionImageHeight
        {
            get { return _compositionImageHeight; }
            set
            {
                _compositionImageHeight = value;
                OnPropertyChanged(() => CompositionImageHeight);
            }
        }

        private Visibility _compositionImageVisibility = Visibility.Collapsed;
        public Visibility CompositionImageVisibility
        {
            get { return _compositionImageVisibility; }
            set
            {
                _compositionImageVisibility = value;
                OnPropertyChanged(() => CompositionImageVisibility);
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(() => Message);
            }
        }

		private double _compositionPanelTop;
		public double CompositionPanelTop
		{
			get { return _compositionPanelTop; }
			set
			{
				_compositionPanelTop = value;
				OnPropertyChanged(() => CompositionPanelTop);
			}
		}

		private double _compositionPanelLeft;
		public double CompositionPanelLeft
		{
			get { return _compositionPanelLeft; }
			set
			{
				_compositionPanelLeft = value;
				OnPropertyChanged(() => CompositionPanelLeft);
			}
		}

        private Visibility _sidebarCoverVisibility = Visibility.Collapsed;
        public Visibility SidebarCoverVisibility
        {
            get
            {
                return _sidebarCoverVisibility;
            }
            set
            {
                _sidebarCoverVisibility = value;
                OnPropertyChanged(() => SidebarCoverVisibility);
            }
        }

		public void OnPlaceCompositionPanel(Point p)
		{
            //CompositionPanelLeft, CompositionPanelTop, only control the position of the CompositionView when the application is loading. after the CompositionView
            //loads, it appears in the correct location no matter what the values of CompositionPanelLeft and CompositionPanelTop are.

            //since HubView is a child of CompositionView, CompositionPanelLeft and CompositionPanelTop do affect the location 
            //where HubView appears, but HubView can control its own positioning, limiting the apparent usefullness of this handler.

            //TODO: evaluate necessity of this handler.

			var dX = EditorState.ViewportWidth - p.X;
			var dY = EditorState.ViewportHeight - p.Y;
            CompositionPanelLeft = 380;
            CompositionPanelTop = 30;
		}

		public void OnResizeViewPort(Point coordinate)
		{
			EditorState.ViewportHeight = coordinate.Y;
			EditorState.ViewportWidth = coordinate.X;
		}

        public void OnToggleSidebarVisibility(Visibility visibility)
        {
            SidebarVisibility = visibility;
        }

        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            MouseEventArgs e;
            if (commandParameter.EventArgs.GetType() == typeof(MouseEventArgs))
            {
                e = commandParameter.EventArgs as MouseEventArgs;
                if (commandParameter.Parameter != null)
                {
                    var view = commandParameter.Parameter as UIElement;
                }
            }
        }

        public void OnMouseMoveSidebar(ExtendedCommandParameter commandParameter)
        {

        }

        private void OnMouseLeftButtonUpCommand(object o)
        {

        }

        private void OnCloseException(object o)
        {
            ExceptionMessageVisibility = Visibility.Collapsed;
        }

        private void OnLogException(object o)
        {

        }

		public void OnDisplayExceptionMessage(string message)
		{
			ExceptionMessage += message + Environment.NewLine + "***************************" + Environment.NewLine;
			ExceptionMessageVisibility = Visibility.Visible;
		}

		public void OnShowCollaborationPanel(object obj)
		{
            EA.GetEvent<SuspendEditing>().Publish(string.Empty);
            EA.GetEvent<BlurComposition>().Publish(0);
			EditorState.Collaborating = true;
			CollaborationsVisibility = Visibility.Visible;
		}

		public void OnHideCollaborationPanel(object obj)
		{
            EA.GetEvent<ResumeEditing>().Publish(string.Empty);
			EditorState.Collaborating = false;
			CollaborationsVisibility = Visibility.Collapsed;
			EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
		}

		public void OnUpdateCollaboratorName(string s)
		{
			CollaboratorNameVisibility = (s.Length > 0) ? Visibility.Visible : Visibility.Collapsed;
			CollaboratorName = s;
		}

		public void OnUiScaleChanged(double newScale)
		{
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
		}

		public void OnResumeEditing(object obj)
		{
			EditorState.IsComposing = true;
            this.SidebarCoverVisibility = Visibility.Collapsed;
			EA.GetEvent<HideBusyIndicator>().Publish(string.Empty);
		}

		public void OnHideBusyIndicator(object obj)
		{
			BusyIndicatorVisibility = Visibility.Collapsed;
		}

		public void OnShowBusyIndicator(object obj)
		{
			EA.GetEvent<DeSelectAll>().Publish(string.Empty);
			BusyIndicatorVisibility = Visibility.Visible;
		}

		public void OnSuspendEditing(object obj)
		{
			EditorState.IsComposing = false;
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
            this.SidebarCoverVisibility = Visibility.Visible;
		}

		public void OnToolSelected(string tool)
		{
			if (string.IsNullOrEmpty(tool))
				return;
			SelectedTool = tool;
		}

		private Visibility _printingVisibility;
		public Visibility PrintingVisibility
		{
			get { return _printingVisibility; }
			set
			{
				_printingVisibility = value;
				OnPropertyChanged(() => PrintingVisibility);
                PrintButtonVisibility = Visibility.Collapsed;
			}
		}

		private Visibility _compositionVisibility;
		public Visibility CompositionVisibility
		{
			get { return _compositionVisibility; }
			set
			{
				_compositionVisibility = value;
				OnPropertyChanged(() => CompositionVisibility);
			}
		}

        private Visibility _sidebarVisibility;
        public Visibility SidebarVisibility
        {
            get { return _sidebarVisibility; }
            set
            {
                _sidebarVisibility = value;
                OnPropertyChanged(() => SidebarVisibility);
            }
        }

		private string _exceptionMessage = string.Empty;
		public string ExceptionMessage
		{
			get { return _exceptionMessage; }
			set
			{
				_exceptionMessage = value;
				OnPropertyChanged(() => ExceptionMessage);
			}
		}

		private string _collaboratorName = string.Empty;
		public string CollaboratorName
		{
			get { return _collaboratorName; }
			set
			{
				_collaboratorName = value;
				OnPropertyChanged(() => CollaboratorName);
			}
		}

		private Visibility _collaboratorNameVisibility = Visibility.Collapsed;
		public Visibility CollaboratorNameVisibility
		{
			get { return _collaboratorNameVisibility; }
			set
			{
				_collaboratorNameVisibility = value;
				OnPropertyChanged(() => CollaboratorNameVisibility);
			}
		}

		private int _busyIndicatorX;
		public int BusyIndicator_X
		{
			get { return _busyIndicatorX; }
			set
			{
				_busyIndicatorX = value;
				OnPropertyChanged(() => BusyIndicator_X);
			}
		}

		private int _busyIndicatorY;
		public int BusyIndicator_Y
		{
			get { return _busyIndicatorY; }
			set
			{
				_busyIndicatorY = value;
				OnPropertyChanged(() => BusyIndicator_Y);
			}
		}

        private double _transposePanelX;
        public double TransposePanel_X
        {
            get { return _transposePanelX; }
            set
            {
                if (Math.Abs(_transposePanelX - value) > 0)
                {
                    _transposePanelX = value;
                    OnPropertyChanged(() => TransposePanel_X);
                }
            }
        }

        private double _transposePanelY;
        public double TransposePanel_Y
        {
            get { return _transposePanelY;  }
            set
            {
                if (Math.Abs(_transposePanelY - value) > 0)
                {
                    _transposePanelY = value;
                    OnPropertyChanged(() => TransposePanel_Y);
                }
            }
        }

        private Visibility _transposePanelVisibility = Visibility.Visible;
        public Visibility TransposePanelVisibility
        {
            get { return _transposePanelVisibility; }
            set
            {
                _transposePanelVisibility = value;
                OnPropertyChanged(() => TransposePanelVisibility);
            }
        }

        private double _noteEditorX;
        public double NoteEditor_X
        {
            get { return _noteEditorX; }
            set
            {
                if (Math.Abs(_noteEditorX - value) > 0)
                {
                    _noteEditorX = value;
                    OnPropertyChanged(() => NoteEditor_X);
                }
            }
        }

        private double _noteEditorY;
        public double NoteEditor_Y
        {
            get { return _noteEditorY; }
            set
            {
                if (Math.Abs(_noteEditorY - value) > 0)
                {
                    _noteEditorY = value;
                    OnPropertyChanged(() => NoteEditor_Y);
                }
            }
        }

        private Visibility _noteEditorVisibility = Visibility.Collapsed;
        public Visibility NoteEditorVisibility
        {
            get { return _noteEditorVisibility; }
            set
            {
                _noteEditorVisibility = value;
                OnPropertyChanged(() => NoteEditorVisibility);
            }
        }

        private double _savePanelX;
        public double SavePanel_X
        {
            get { return _savePanelX; }
            set
            {
                if (Math.Abs(_savePanelX - value) > 0)
                {
                    _savePanelX = value;
                    OnPropertyChanged(() => SavePanel_X);
                }
            }
        }

        private double _savePanelY;
        public double SavePanel_Y
        {
            get { return _savePanelY; }
            set
            {
                if (Math.Abs(_savePanelY - value) > 0)
                {
                    _savePanelY = value;
                    OnPropertyChanged(() => SavePanel_Y);
                }
            }
        }

        private Visibility _savePanelVisibility = Visibility.Visible;
        public Visibility SavePanelVisibility
        {
            get
            {
                return _savePanelVisibility;
            }
            set
            {
                _savePanelVisibility = value;
                OnPropertyChanged(() => SavePanelVisibility);
            }
        }

        private double _lyricsPanelX;
        public double LyricsPanel_X
        {
            get { return _lyricsPanelX; }
            set
            {
                if (Math.Abs(_lyricsPanelX - value) > 0)
                {
                    _lyricsPanelX = value;
                    OnPropertyChanged(() => LyricsPanel_X);
                }
            }
        }

        private double _lyricsPanelY;
        public double LyricsPanel_Y
        {
            get { return _lyricsPanelY; }
            set
            {
                if (Math.Abs(_lyricsPanelY - value) > 0)
                {
                    _lyricsPanelY = value;
                    OnPropertyChanged(() => LyricsPanel_Y);
                }
            }
        }

        private Visibility _lyricsPanelVisibility = Visibility.Visible;
        public Visibility LyricsPanelVisibility
        {
            get { return _lyricsPanelVisibility; }
            set
            {
                _lyricsPanelVisibility = value;
                OnPropertyChanged(() => LyricsPanelVisibility);
            }
        }

        private double _barsX;
        public double Bars_X
        {
            get { return _barsX; }
            set
            {
                if (Math.Abs(_barsX - value) > 0)
                {
                    _barsX = value;
                    OnPropertyChanged(() => Bars_X);
                }
            }
        }

        private double _barsY;
        public double Bars_Y
        {
            get { return _barsY; }
            set
            {
                if (Math.Abs(_barsY - value) > 0)
                {
                    _barsY = value;
                    OnPropertyChanged(() => Bars_Y);
                }
            }
        }

        private Visibility _barsVisibility = Visibility.Visible;
        public Visibility BarsVisibility
        {
            get { return _barsVisibility; }
            set
            {
                _barsVisibility = value;
                OnPropertyChanged(() => BarsVisibility);
            }
        }

        private double _newCompositionPanelX;
        public double NewCompositionPanel_X
        {
            get { return _newCompositionPanelX; }
            set
            {
                if (Math.Abs(_newCompositionPanelX - value) > 0)
                {
                    _newCompositionPanelX = value;
                    OnPropertyChanged(() => NewCompositionPanel_X);
                }
            }
        }

        private double _newCompositionPanelY;
        public double NewCompositionPanel_Y
        {
            get { return _newCompositionPanelY; }
            set
            {
                if (Math.Abs(_newCompositionPanelY - value) > 0)
                {
                    _newCompositionPanelY = value;
                    OnPropertyChanged(() => NewCompositionPanel_Y);
                }
            }
        }

        private Visibility newCompositionPanelVisibility = Visibility.Collapsed;
        public Visibility NewCompositionPanelVisibility
        {
            get { return newCompositionPanelVisibility; }
            set
            {
                newCompositionPanelVisibility = value;
                OnPropertyChanged(() => NewCompositionPanelVisibility);
            }
        }

		private Visibility _printButtonVisibility = Visibility.Collapsed;
		public Visibility PrintButtonVisibility
		{
			get { return _printButtonVisibility; }
			set
			{
				_printButtonVisibility = value;
				OnPropertyChanged(() => PrintButtonVisibility);
			}
		}

		private Visibility _busyIndicatorVisibility;
		public Visibility BusyIndicatorVisibility
		{
			get { return _busyIndicatorVisibility; }
			set
			{
				_busyIndicatorVisibility = value;
				OnPropertyChanged(() => BusyIndicatorVisibility);
			}
		}

		private Visibility _collaborationsVisibility;
		public Visibility CollaborationsVisibility
		{
			get { return _collaborationsVisibility; }
			set
			{
				_collaborationsVisibility = value;
				OnPropertyChanged(() => CollaborationsVisibility);
			}
		}

		private Visibility _exceptionMessageVisibility;
		public Visibility ExceptionMessageVisibility
		{
			get { return _exceptionMessageVisibility; }
			set
			{
				_exceptionMessageVisibility = value;
				OnPropertyChanged(() => ExceptionMessageVisibility);
			}
		}

		private int _exceptionMessageX;
		public int ExceptionMessage_X
		{
			get { return _exceptionMessageX; }
			set
			{
				_exceptionMessageX = value;
				OnPropertyChanged(() => ExceptionMessage_X);
			}
		}

		private int _exceptionMessageY;
		public int ExceptionMessage_Y
		{
			get { return _exceptionMessageY; }
			set
			{
				_exceptionMessageY = value;
				OnPropertyChanged(() => ExceptionMessage_Y);
			}
		}

		private int _collaborationsX;
		public int Collaborations_X
		{
			get { return _collaborationsX; }
			set
			{
				_collaborationsX = value;
				OnPropertyChanged(() => Collaborations_X);
			}
		}

		private int _collaborationsY;
		public int Collaborations_Y
		{
			get { return _collaborationsY; }
			set
			{
				_collaborationsY = value;
				OnPropertyChanged(() => Collaborations_Y);
			}
		}

		private string _selectedTool;
		public string SelectedTool
		{
			get { return _selectedTool; }
			set
			{
				if (value != _selectedTool)
				{
					_selectedTool = value;
					OnPropertyChanged(() => SelectedTool);
				}
			}
		}

	    private ICommand _logExceptionLeftButtonUpCommand;
		public ICommand LogExceptionLeftButtonUpCommand
		{
			get { return _logExceptionLeftButtonUpCommand; }
			set
			{
				if (value != _logExceptionLeftButtonUpCommand)
				{
					_logExceptionLeftButtonUpCommand = value;
					OnPropertyChanged(() => LogExceptionLeftButtonUpCommand);
				}
			}
		}

		private ICommand _closeExceptionLeftButtonUpCommand;
		public ICommand CloseExceptionLeftButtonUpCommand
		{
			get { return _closeExceptionLeftButtonUpCommand; }
			set
			{
				if (value != _closeExceptionLeftButtonUpCommand)
				{
					_closeExceptionLeftButtonUpCommand = value;
					OnPropertyChanged(() => CloseExceptionLeftButtonUpCommand);
				}
			}
		}

		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveCommand;
		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveCommand
		{
			get { return _mouseMoveCommand; }
			set
			{
				if (value != _mouseMoveCommand)
				{
					_mouseMoveCommand = value;
					OnPropertyChanged(() => MouseMoveCommand);
				}
			}
		}

		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveSidebarCommand;
		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveSidebarCommand
		{
			get { return _mouseMoveSidebarCommand; }
			set
			{
				if (value != _mouseMoveSidebarCommand)
				{
					_mouseMoveSidebarCommand = value;
					OnPropertyChanged(() => MouseMoveSidebarCommand);
				}
			}
		}

		private ICommand _mouseLeftButonUpCommand;
		public ICommand MouseLeftButtonUpCommand
		{
			get { return _mouseLeftButonUpCommand; }
			set
			{
				if (value != _mouseLeftButonUpCommand)
				{
					_mouseLeftButonUpCommand = value;
					OnPropertyChanged(() => MouseLeftButtonUpCommand);
				}
			}
		}

        private DelegatedCommand<object> printCommand;
        public DelegatedCommand<object> PrintCommand
        {
            get { return printCommand; }
            set
            {
                if (value != printCommand)
                {
                    printCommand = value;
                    OnPropertyChanged(() => PrintCommand);
                }
            }
        }

		private double _editPopupX;
		public double EditPopup_X
		{
			get { return _editPopupX; }
			set
			{
				if (Math.Abs(_editPopupX - value) > 0)
				{
					_editPopupX = value + 35;
					OnPropertyChanged(() => EditPopup_X);
				}
			}
		}

		private double _editPopupY;
		public double EditPopup_Y
		{
			get { return _editPopupY; }
			set
			{
				if (Math.Abs(_editPopupY - value) > 0)
				{
					_editPopupY = value;
					OnPropertyChanged(() => EditPopup_Y);
				}
			}
		}

		private Visibility _editPopupVisibility = Visibility.Visible;
		public Visibility EditPopupVisibility
		{
			get { return _editPopupVisibility; }
			set
			{
				_editPopupVisibility = value;
				OnPropertyChanged(() => EditPopupVisibility);
			}
		}
	}
}