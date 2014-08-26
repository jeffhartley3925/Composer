namespace Composer.Modules.Composition.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;

	using Composer.Infrastructure;
	using Composer.Infrastructure.Behavior;
	using Composer.Infrastructure.Events;
	using Composer.Modules.Composition.ViewModels.Helpers;

	public class NoteDispositionViewModel : BaseViewModel, INoteDispositionViewModel, IEventCatcher
	{
		private Guid NoteId;

		private const string AcceptForeground = "#ffffff";
		private static readonly string AcceptBackground = Application.Current.Resources["DarkGreen"].ToString();
		private const string RejectForeground = "#ffffff";
		private static readonly string RejectBackground = Application.Current.Resources["DarkRed"].ToString();
		private _Enum.Disposition _disposition = _Enum.Disposition.Na;
		private _Enum.DispositionLocation _dispositionLocation = _Enum.DispositionLocation.SideVertical;

		private Repository.DataService.Chord Chord;

		private Repository.DataService.Note Note;

		public NoteDispositionViewModel(string nTiD)
		{
			NoteId = Guid.Parse(nTiD);
			Note = Utils.GetNote(NoteId);
			Chord = Utils.GetChord(Note.Chord_Id);
			GetDispositionLocation();
			OnSetDispositionButtonProperties(Note);
		}

		private void ArrangeDispositionButtons()
		{
			switch (_dispositionLocation)
			{
				case _Enum.DispositionLocation.SideHorizontal:
					switch (_disposition)
					{
						case _Enum.Disposition.Reject:
							AcceptColumn = 1;
							AcceptRow = 0;
							RejectColumn = 0;
							RejectRow = 0;
							break;
						default:
							AcceptColumn = 0;
							AcceptRow = 0;
							RejectColumn = 1;
							RejectRow = 0;
							break;
					}
					break;
				case _Enum.DispositionLocation.SideVertical:
					switch (_disposition)
					{
						case _Enum.Disposition.Reject:
							AcceptColumn = 0;
							AcceptRow = 1;
							RejectColumn = 0;
							RejectRow = 0;
							break;
						default:
							AcceptColumn = 0;
							AcceptRow = 0;
							RejectColumn = 0;
							RejectRow = 1;
							break;
					}
					break;
			}
		}

		private void SetDispositionMargin()
		{
			switch (_dispositionLocation)
			{
				case _Enum.DispositionLocation.SideHorizontal:
					DispositionMargin = "10,18,0,0";
					break;
				case _Enum.DispositionLocation.SideVertical:
					DispositionMargin = "10,19,0,0";
					break;
				case _Enum.DispositionLocation.BottomHorizontal:
					DispositionMargin = "10,19,0,0";
					break;
				case _Enum.DispositionLocation.BottomVertical:
					DispositionMargin = "10,19,0,0";
					break;
			}
		}

		private void GetDispositionLocation()
		{
			// the note disposition buttons can easily be partially covered up by other composition elements, so
			// we must examine each note in relation to its surrounding to detemine the best place to situate the buttons.
			_dispositionLocation = _Enum.DispositionLocation.SideVertical;
			if (Chord.Notes.Count() > 1)
			{
				_dispositionLocation = _Enum.DispositionLocation.SideHorizontal;
			}
			SetDispositionMargin();
			ArrangeDispositionButtons();
		}

		private int _acceptRow = 0;
		public int AcceptRow
		{
			get { return _acceptRow; }
			set
			{
				_acceptRow = value;
				OnPropertyChanged(() => AcceptRow);
			}
		}

		private int _rejectRow = 1;
		public int RejectRow
		{
			get { return _rejectRow; }
			set
			{
				_rejectRow = value;
				OnPropertyChanged(() => RejectRow);
			}
		}

		private int _acceptColumn = 0;
		public int AcceptColumn
		{
			get { return _acceptColumn; }
			set
			{
				_acceptColumn = value;
				OnPropertyChanged(() => AcceptColumn);
			}
		}

		private int _rejectColumn = 0;
		public int RejectColumn
		{
			get { return _rejectColumn; }
			set
			{
				_rejectColumn = value;
				OnPropertyChanged(() => RejectColumn);
			}
		}

		private double _acceptOpacity = .8;
		public double AcceptOpacity
		{
			get { return _acceptOpacity; }
			set
			{
				_acceptOpacity = value;
				OnPropertyChanged(() => AcceptOpacity);
			}
		}

		private double _rejectOpacity = .8;
		public double RejectOpacity
		{
			get { return _rejectOpacity; }
			set
			{
				_rejectOpacity = value;
				OnPropertyChanged(() => RejectOpacity);
			}
		}

		private double _dispositionButtonHeight;
		public double DispositionButtonHeight
		{
			get { return _dispositionButtonHeight; }
			set
			{
				_dispositionButtonHeight = value;
				OnPropertyChanged(() => DispositionButtonHeight);
			}
		}

		private double _dispositionButtonWidth;
		public double DispositionButtonWidth
		{
			get { return _dispositionButtonWidth; }
			set
			{
				_dispositionButtonWidth = value;
				OnPropertyChanged(() => DispositionButtonWidth);
			}
		}

		private Visibility _dispositionVisibility = Visibility.Visible;
		public Visibility DispositionVisibility
		{
			get { return _dispositionVisibility; }
			set
			{
				_dispositionVisibility = value;
				DispositionButtonWidth = (_dispositionVisibility == Visibility.Collapsed) ? 0 : 20;
				DispositionButtonHeight = (_dispositionVisibility == Visibility.Collapsed) ? 0 : 20;
				OnPropertyChanged(() => DispositionVisibility);
			}
		}

		private double _dispositionScale = 1;
		public double DispositionScale
		{
			get { return _dispositionScale; }
			set
			{
				if (Math.Abs(value - _dispositionScale) > 0)
				{
					_dispositionScale = value;
					OnPropertyChanged(() => DispositionScale);
				}
			}
		}

		private string _dispositionMargin;
		public string DispositionMargin
		{
			get { return _dispositionMargin; }
			set
			{
				if (value != _dispositionMargin)
				{
					_dispositionMargin = value;
					OnPropertyChanged(() => DispositionMargin);
				}
			}
		}

		private int _dispositionStrokeThickness = 1;
		public int DispositionStrokeThickness
		{
			get { return _dispositionStrokeThickness; }
			set
			{
				if (value != _dispositionStrokeThickness)
				{
					_dispositionStrokeThickness = value;
					OnPropertyChanged(() => DispositionStrokeThickness);
				}
			}
		}

		private string _dispositionAcceptBackground = AcceptBackground;
		public string DispositionAcceptBackground
		{
			get { return _dispositionAcceptBackground; }
			set
			{
				if (value != _dispositionAcceptBackground)
				{
					_dispositionAcceptBackground = value;
					OnPropertyChanged(() => DispositionAcceptBackground);
				}
			}
		}

		private string _dispositionRejectBackground = RejectBackground;
		public string DispositionRejectBackground
		{
			get { return _dispositionRejectBackground; }
			set
			{
				if (value != _dispositionRejectBackground)
				{
					_dispositionRejectBackground = value;
					OnPropertyChanged(() => DispositionRejectBackground);
				}
			}
		}

		private string _dispositionAcceptForeground = AcceptForeground;
		public string DispositionAcceptForeground
		{
			get { return _dispositionAcceptForeground; }
			set
			{
				if (value != _dispositionAcceptForeground)
				{
					_dispositionAcceptForeground = value;
					OnPropertyChanged(() => DispositionAcceptForeground);
				}
			}

		}

		private string _dispositionRejectForeground = RejectForeground;
		public string DispositionRejectForeground
		{
			get { return _dispositionRejectForeground; }
			set
			{
				if (value != _dispositionRejectForeground)
				{
					_dispositionRejectForeground = value;
					OnPropertyChanged(() => DispositionRejectForeground);
				}
			}
		}


		public void OnClickAccept(Guid id)
		{
			if (Note.Id == id)
			{
				#region UI Adjustments

				DispositionAcceptBackground = AcceptBackground;
				DispositionAcceptForeground = AcceptForeground;

				switch (_disposition)
				{
					case _Enum.Disposition.Na:
						AcceptOpacity = 1;
						RejectOpacity = .1;
						_disposition = _Enum.Disposition.Accept;
						break;
					case _Enum.Disposition.Accept:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						_disposition = _Enum.Disposition.Na;
						break;
					case _Enum.Disposition.Reject:
						AcceptOpacity = 1;
						RejectOpacity = .1;
						_disposition = _Enum.Disposition.Accept;
						break;
					default:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						break;
				}
				ArrangeDispositionButtons();

				#endregion

				SetDisposition(_disposition, id);
			}
		}

		public void OnClickReject(Guid id)
		{
			if (Note.Id == id)
			{
				#region UI Adjustments

				DispositionRejectBackground = RejectBackground;
				DispositionRejectForeground = RejectForeground;

				switch (_disposition)
				{
					case _Enum.Disposition.Na:
						AcceptOpacity = .1;
						RejectOpacity = 1;
						_disposition = _Enum.Disposition.Reject;

						break;
					case _Enum.Disposition.Accept:
						AcceptOpacity = .1;
						RejectOpacity = 1;
						_disposition = _Enum.Disposition.Reject;
						break;
					case _Enum.Disposition.Reject:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						_disposition = _Enum.Disposition.Na;
						break;
					default:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						break;
				}

				ArrangeDispositionButtons();

				#endregion

				SetDisposition(_disposition, id);
			}
		}


		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownRejectCommand;

		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownRejectCommand
		{
			get
			{
				return _mouseLeftButtonDownRejectCommand;
			}
			set
			{
				if (value != _mouseLeftButtonDownRejectCommand)
				{
					_mouseLeftButtonDownRejectCommand = value;
					OnPropertyChanged(() => MouseLeftButtonDownRejectCommand);
				}
			}
		}

		public void OnMouseLeftButtonDownReject(ExtendedCommandParameter commandParameter)
		{
			if (_disposition == _Enum.Disposition.Reject)
			{
				RejectOpacity = .5;
			}
			else
			{
				DispositionRejectBackground = RejectBackground;
				DispositionRejectForeground = RejectForeground;
				RejectOpacity = 1;
			}
		}

		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownAcceptCommand;

		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownAcceptCommand
		{
			get
			{
				return _mouseLeftButtonDownAcceptCommand;
			}
			set
			{
				if (value != _mouseLeftButtonDownAcceptCommand)
				{
					_mouseLeftButtonDownAcceptCommand = value;
					OnPropertyChanged(() => MouseLeftButtonDownAcceptCommand);
				}
			}
		}

		public void OnMouseLeftButtonDownAccept(ExtendedCommandParameter commandParameter)
		{
			if (_disposition == _Enum.Disposition.Accept)
			{
				AcceptOpacity = .5;
			}
			else
			{
				DispositionAcceptBackground = AcceptBackground;
				DispositionAcceptForeground = AcceptForeground;
				AcceptOpacity = 1;
			}
		}

		private void SetDisposition(_Enum.Disposition disposition, Guid noteId)
		{
			if (Collaborations.DispositionChanges == null)
			{
				Collaborations.DispositionChanges = new List<DispositionChangeItem>();
			}
			var a = from b in Collaborations.DispositionChanges where b.ItemId == noteId select b;
			var dispositionChangeItems = a as List<DispositionChangeItem> ?? a.ToList();
			if (dispositionChangeItems.Any())
			{
				DispositionChangeItem item = dispositionChangeItems.SingleOrDefault();
				if (item != null) item.Disposition = _disposition;
			}
			else
			{
				NoteController.AddDispositionChangeItem(Note, Note, disposition);
			}
			var c = from d in Collaborations.DispositionChanges where d.Disposition != _Enum.Disposition.Na select d;
			EA.GetEvent<UpdateCollaborationPanelSaveButtonEnableState>().Publish(c.Any());
		}

		public void SubscribeEvents()
		{
			EA.GetEvent<AcceptClick>().Subscribe(OnClickAccept);
			EA.GetEvent<RejectClick>().Subscribe(OnClickReject);
		}
		
        public void OnSetDispositionButtonProperties(Repository.DataService.Note n)
        {
            if (CollaborationManager.IsPendingDelete(Collaborations.GetStatus(n)))
            {
                if (Collaborations.CurrentCollaborator != null)
                {
                    if (n.Audit.CollaboratorIndex == -1 ||
                        n.Audit.CollaboratorIndex == Collaborations.CurrentCollaborator.Index)
                    {
                        //OnShowDispositionButtons();
                        n.Foreground = Preferences.DeletedColor;
                    }
                }
            }
            else
            {
                if ((CollaborationManager.IsPendingAdd(Collaborations.GetStatus(n))))
                {

                    //OnShowDispositionButtons();
                    n.Foreground = Preferences.AddedColor;
                }
                else
                {
                    EA.GetEvent<HideDispositionButtons>().Publish(string.Empty);
                    n.Foreground = Preferences.NoteForeground;
                }
            }
        }

		public void OnShowDispositionButtons()
		{
			DispositionVisibility = Visibility.Visible;
		}

		public void OnHideDispositionButtons(object obj)
		{
			DispositionVisibility = Visibility.Visible;
		}

		public void DefineCommands()
		{
			MouseLeftButtonDownAcceptCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownAccept, null);
			MouseLeftButtonDownRejectCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownReject, null);
		}

		public bool IsTargetVM(System.Guid Id)
		{
			throw new System.NotImplementedException();
		}
	}
}
