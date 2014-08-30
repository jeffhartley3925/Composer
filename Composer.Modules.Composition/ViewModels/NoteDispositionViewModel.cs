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
		private const string AcceptForeground = "#ffffff";
		private static readonly string AcceptBackground = Application.Current.Resources["DarkGreen"].ToString();
		private const string RejectForeground = "#ffffff";
		private static readonly string RejectBackground = Application.Current.Resources["DarkRed"].ToString();
		private _Enum.Disposition disposition = _Enum.Disposition.Na;
		private _Enum.DispositionLocation dispositionLocation = _Enum.DispositionLocation.SideVertical;

		private Repository.DataService.Chord Chord;

		private Repository.DataService.Note Note;

		public NoteDispositionViewModel(string nTiD)
		{
			Guid noteId = Guid.Parse(nTiD);
			Note = Utils.GetNote(noteId);
			Chord = Utils.GetChord(Note.Chord_Id);
			this.SubscribeEvents();
			GetDispositionLocation();
		}

		private void ArrangeDispositionButtons()
		{
			switch (this.dispositionLocation)
			{
				case _Enum.DispositionLocation.SideHorizontal:
					switch (this.disposition)
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
					switch (this.disposition)
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
			switch (this.dispositionLocation)
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
			this.dispositionLocation = _Enum.DispositionLocation.SideVertical;
			if (Chord.Notes.Count() > 1)
			{
				this.dispositionLocation = _Enum.DispositionLocation.SideHorizontal;
			}
			SetDispositionMargin();
			ArrangeDispositionButtons();
		}

		private int acceptRow;
		public int AcceptRow
		{
			get { return this.acceptRow; }
			set
			{
				this.acceptRow = value;
				OnPropertyChanged(() => AcceptRow);
			}
		}

		private int rejectRow = 1;
		public int RejectRow
		{
			get { return this.rejectRow; }
			set
			{
				this.rejectRow = value;
				OnPropertyChanged(() => RejectRow);
			}
		}

		private int acceptColumn;
		public int AcceptColumn
		{
			get { return this.acceptColumn; }
			set
			{
				this.acceptColumn = value;
				OnPropertyChanged(() => AcceptColumn);
			}
		}

		private int rejectColumn;
		public int RejectColumn
		{
			get { return this.rejectColumn; }
			set
			{
				this.rejectColumn = value;
				OnPropertyChanged(() => RejectColumn);
			}
		}

		private double acceptOpacity = .8;
		public double AcceptOpacity
		{
			get { return this.acceptOpacity; }
			set
			{
				this.acceptOpacity = value;
				OnPropertyChanged(() => AcceptOpacity);
			}
		}

		private double rejectOpacity = .8;
		public double RejectOpacity
		{
			get { return this.rejectOpacity; }
			set
			{
				this.rejectOpacity = value;
				OnPropertyChanged(() => RejectOpacity);
			}
		}

		private double dispositionButtonHeight;
		public double DispositionButtonHeight
		{
			get { return this.dispositionButtonHeight; }
			set
			{
				this.dispositionButtonHeight = value;
				OnPropertyChanged(() => DispositionButtonHeight);
			}
		}

		private double dispositionButtonWidth;
		public double DispositionButtonWidth
		{
			get { return this.dispositionButtonWidth; }
			set
			{
				this.dispositionButtonWidth = value;
				OnPropertyChanged(() => DispositionButtonWidth);
			}
		}

		private Visibility dispositionVisibility = Visibility.Visible;
		public Visibility DispositionVisibility
		{
			get { return this.dispositionVisibility; }
			set
			{
				this.dispositionVisibility = value;
				DispositionButtonWidth = (this.dispositionVisibility == Visibility.Collapsed) ? 0 : 20;
				DispositionButtonHeight = (this.dispositionVisibility == Visibility.Collapsed) ? 0 : 20;
				OnPropertyChanged(() => DispositionVisibility);
			}
		}

		private double dispositionScale = 1;
		public double DispositionScale
		{
			get { return this.dispositionScale; }
			set
			{
				if (Math.Abs(value - this.dispositionScale) > 0)
				{
					this.dispositionScale = value;
					OnPropertyChanged(() => DispositionScale);
				}
			}
		}

		private string dispositionMargin;
		public string DispositionMargin
		{
			get { return this.dispositionMargin; }
			set
			{
				if (value != this.dispositionMargin)
				{
					this.dispositionMargin = value;
					OnPropertyChanged(() => DispositionMargin);
				}
			}
		}

		private int dispositionStrokeThickness = 1;
		public int DispositionStrokeThickness
		{
			get { return this.dispositionStrokeThickness; }
			set
			{
				if (value != this.dispositionStrokeThickness)
				{
					this.dispositionStrokeThickness = value;
					OnPropertyChanged(() => DispositionStrokeThickness);
				}
			}
		}

		private string dispositionAcceptBackground = AcceptBackground;
		public string DispositionAcceptBackground
		{
			get { return this.dispositionAcceptBackground; }
			set
			{
				if (value != this.dispositionAcceptBackground)
				{
					this.dispositionAcceptBackground = value;
					OnPropertyChanged(() => DispositionAcceptBackground);
				}
			}
		}

		private string dispositionRejectBackground = RejectBackground;
		public string DispositionRejectBackground
		{
			get { return this.dispositionRejectBackground; }
			set
			{
				if (value != this.dispositionRejectBackground)
				{
					this.dispositionRejectBackground = value;
					OnPropertyChanged(() => DispositionRejectBackground);
				}
			}
		}

		private string dispositionAcceptForeground = AcceptForeground;
		public string DispositionAcceptForeground
		{
			get { return this.dispositionAcceptForeground; }
			set
			{
				if (value != this.dispositionAcceptForeground)
				{
					this.dispositionAcceptForeground = value;
					OnPropertyChanged(() => DispositionAcceptForeground);
				}
			}
		}

		private string dispositionRejectForeground = RejectForeground;
		public string DispositionRejectForeground
		{
			get { return this.dispositionRejectForeground; }
			set
			{
				if (value != this.dispositionRejectForeground)
				{
					this.dispositionRejectForeground = value;
					OnPropertyChanged(() => DispositionRejectForeground);
				}
			}
		}

		public void OnClickAccept(Guid id)
		{
			if (this.IsTargetVM(id))
			{
				#region UI Adjustments

				DispositionAcceptBackground = AcceptBackground;
				DispositionAcceptForeground = AcceptForeground;

				switch (this.disposition)
				{
					case _Enum.Disposition.Na:
						AcceptOpacity = 1;
						RejectOpacity = .1;
						this.disposition = _Enum.Disposition.Accept;
						break;
					case _Enum.Disposition.Accept:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						this.disposition = _Enum.Disposition.Na;
						break;
					case _Enum.Disposition.Reject:
						AcceptOpacity = 1;
						RejectOpacity = .1;
						this.disposition = _Enum.Disposition.Accept;
						break;
					default:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						break;
				}
				ArrangeDispositionButtons();

				#endregion

				SetDisposition(this.disposition, id);
			}
		}

		public void OnClickReject(Guid id)
		{
			if (this.IsTargetVM(id))
			{
				#region UI Adjustments

				DispositionRejectBackground = RejectBackground;
				DispositionRejectForeground = RejectForeground;

				switch (this.disposition)
				{
					case _Enum.Disposition.Na:
						AcceptOpacity = .1;
						RejectOpacity = 1;
						this.disposition = _Enum.Disposition.Reject;

						break;
					case _Enum.Disposition.Accept:
						AcceptOpacity = .1;
						RejectOpacity = 1;
						this.disposition = _Enum.Disposition.Reject;
						break;
					case _Enum.Disposition.Reject:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						this.disposition = _Enum.Disposition.Na;
						break;
					default:
						AcceptOpacity = .3;
						RejectOpacity = .3;
						break;
				}

				ArrangeDispositionButtons();

				#endregion

				SetDisposition(this.disposition, id);
			}
		}

		private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeftButtonDownRejectCommand;

		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownRejectCommand
		{
			get
			{
				return this.mouseLeftButtonDownRejectCommand;
			}
			set
			{
				if (value != this.mouseLeftButtonDownRejectCommand)
				{
					this.mouseLeftButtonDownRejectCommand = value;
					OnPropertyChanged(() => MouseLeftButtonDownRejectCommand);
				}
			}
		}

		public void OnMouseLeftButtonDownReject(ExtendedCommandParameter commandParameter)
		{
			if (this.disposition == _Enum.Disposition.Reject)
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

		private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeftButtonDownAcceptCommand;

		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownAcceptCommand
		{
			get
			{
				return this.mouseLeftButtonDownAcceptCommand;
			}
			set
			{
				if (value != this.mouseLeftButtonDownAcceptCommand)
				{
					this.mouseLeftButtonDownAcceptCommand = value;
					OnPropertyChanged(() => MouseLeftButtonDownAcceptCommand);
				}
			}
		}

		public void OnMouseLeftButtonDownAccept(ExtendedCommandParameter commandParameter)
		{
			if (this.disposition == _Enum.Disposition.Accept)
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

		private void SetDisposition(_Enum.Disposition dI, Guid nTiD)
		{
			if (Collaborations.DispositionChanges == null)
			{
				Collaborations.DispositionChanges = new List<DispositionChangeItem>();
			}
			var a = from b in Collaborations.DispositionChanges where b.ItemId == nTiD select b;
			var dispositionChangeItems = a as List<DispositionChangeItem> ?? a.ToList();
			if (dispositionChangeItems.Any())
			{
				DispositionChangeItem item = dispositionChangeItems.SingleOrDefault();
				if (item != null) item.Disposition = this.disposition;
			}
			else
			{
				NoteController.AddDispositionChangeItem(Note, Note, dI);
			}
			var c = from d in Collaborations.DispositionChanges where d.Disposition != _Enum.Disposition.Na select d;
			EA.GetEvent<UpdateCollaborationPanelSaveButtonEnableState>().Publish(c.Any());
		}

		public void SubscribeEvents()
		{
			EA.GetEvent<AcceptClick>().Subscribe(OnClickAccept);
			EA.GetEvent<RejectClick>().Subscribe(OnClickReject);
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

		public bool IsTargetVM(Guid nTiD)
		{
			return (Note.Id == nTiD);
		}
	}
}
