using System;
using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Support;
using System.Windows;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
	using System.Windows.Controls;

	public sealed class NoteViewModel : BaseViewModel, INoteViewModel, IEventCatcher
    {
        public long LastTicks = 0;
        public long DeltaTicks = 0;

        public NoteViewModel(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    Debugging = false;
                    EmptyBind = string.Empty;
                    ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
                    var notes = (from n in Cache.Notes where n.Id == Guid.Parse(id) select n);
                    var e = notes as List<Note> ?? notes.ToList();
                    if (e.Any())
                    {
                        Note = e.DefaultIfEmpty(null).Single();
                        if (Note != null)
                        {
                            DefineCommands();
                            SubscribeEvents();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        private Chord _parentChord;
        public Chord ParentChord
        {
            get
            {
                if (_parentChord == null && _note != null)
                {
                    var c = (from a in Cache.Chords where a.Id == _note.Chord_Id select a);
                    var e = c as List<Chord> ?? c.ToList();
                    if (e.Count() == 1)
                    {
                        _parentChord = e.First();
                    }
                }
                return _parentChord;
            }
        }

        private Repository.DataService.Measure _parentMeasure;
        public Repository.DataService.Measure ParentMeasure
        {
            get
            {
                if (_parentMeasure != null) return _parentMeasure;
                var m = Utils.GetMeasure(ParentChord.Measure_Id);
                if (m != null)
                {
                    _parentMeasure = m;
                }
                return _parentMeasure;
            }
        }

        public override bool ShowSelector()
        {
            return base.ShowSelector();
        }

        public override bool HideSelector()
        {
            return base.HideSelector();
        }

        private void SetNotegroupContext()
        {
            NotegroupManager.ChordStarttimes = null;
            NotegroupManager.ChordNotegroups = null;
            NotegroupManager.Measure = ParentMeasure;
            NotegroupManager.Chord = ParentChord;
            NotegroupManager.SetMeasureChordNotegroups();
        }

        #region Bindable Properties
        private Note _note;
        public Note Note
        {
            get { return _note; }
            set
            {
                _note = value;
                PropertiesPanelMargin = (_note.Orientation == (short)_Enum.Direction.Up) ? "-9,33,0,0" : "-9,73,0,0";

                _note.Status = (_note.Status) == null ? "0" : _note.Status;

                Location_Y = value.Location_Y;
                SetLedger();
                Status = _note.Status;
                OnPropertyChanged(() => Note);
            }
        }

        private string _propertiesPanelMargin;
        public string PropertiesPanelMargin
        {
            get { return _propertiesPanelMargin; }
            set
            {
                _propertiesPanelMargin = value;
                OnPropertyChanged(() => PropertiesPanelMargin);
            }
        }

        private Visibility _propertiesPanelVisibility = Visibility.Collapsed;
        public Visibility PropertiesPanelVisibility
        {
            get { return _propertiesPanelVisibility; }
            set
            {
                _propertiesPanelVisibility = value;
                OnPropertyChanged(() => PropertiesPanelVisibility);
            }
        }

        private int _locationY;
        public int Location_Y
        {
            get { return _locationY; }
            set
            {
                _locationY = value;
                OnPropertyChanged(() => Location_Y);
            }
        }
#endregion

        #region Disposition: Fields, Bindable Properties, Methods

        private const string AcceptForeground = "#ffffff";
        private static readonly string AcceptBackground = Application.Current.Resources["DarkGreen"].ToString();
        private const string RejectForeground = "#ffffff";
        private static readonly string RejectBackground = Application.Current.Resources["DarkRed"].ToString();
        private _Enum.Disposition _disposition = _Enum.Disposition.Na;
        private _Enum.DispositionLocation _dispositionLocation = _Enum.DispositionLocation.SideVertical;


		private string _status;
		public string Status
		{
			//not used, but may use later.
			get { return _status; }
			set
			{
				if (_status != value)
				{
					_status = value;
					OnPropertyChanged(() => Status);
				}
			}
		}

        private void AcceptDeletion(int limboStatus, int deletedStatus, short acceptedStatus, Chord chord)
        {
            // either the author is accepting a contributor deletion, or a contributor is accepting a author deletion. either way,
            // the note is gone forever to both contributor and author. Note: Contributor status is set to Purged here, and 
            // Author status is set to Purged at the end of this method.
            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.Purged);

            // If this was the last n of the ch when it was deleted, then there will be
            // a note that is not visible, but needs to be made visible.
            var r = (from a in Cache.Notes
                where
                    Collaborations.GetStatus(a) == limboStatus && // only rests can have a status of 'Limbo'
                    a.StartTime == Note.StartTime
                select a);

            var e = r as List<Note> ?? r.ToList();
            if (e.Any())
            {
                var rE = e.SingleOrDefault();
                if (rE != null)
                {
                    // yes, there is a note, but that doesn't mean we can show the note.
                    // first check if there are other deleted notes pending accept/reject in this chord?
                    var n = (from a in Cache.Notes 
                        where
                            Collaborations.GetStatus(a) == deletedStatus &&
                            a.StartTime == Note.StartTime
                        select a);

                    if (!n.Any() && !CollaborationManager.IsActive(chord)) //this seems to be a double check. each side of the boolean expression implies the other.
                    {
                        rE.Status = Collaborations.SetStatus(rE, acceptedStatus);
                        rE.Status = Collaborations.SetAuthorStatus(rE, (int)_Enum.Status.AuthorOriginal);
                    }
                }
            }
            Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
        }

        #endregion

        #region Bindable Commands, Command Handlers


		public override void OnMouseEnter(ExtendedCommandParameter commandParameter)
		{
			if (!Debugging)
			{
				return;
			}
			PropertiesPanelVisibility = Visibility.Visible;
		}

		public override void OnMouseLeave(ExtendedCommandParameter commandParameter)
		{
			if (!Debugging)
			{
				return;
			}
			PropertiesPanelVisibility = Visibility.Collapsed;
		}

        public void DefineCommands()
        {
            MouseEnterCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnter, null);
            MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, null);


			MouseRightButtonDownCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseRightButtonDown, null);

            ClickCommand = new DelegatedCommand<object>(OnClick);

        }

		private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseRightButtonDownCommand;
		public ExtendedDelegateCommand<ExtendedCommandParameter> MouseRightButtonDownCommand
		{
			get { return _mouseRightButtonDownCommand; }
			set
			{
				if (value != _mouseRightButtonDownCommand)
				{
					_mouseRightButtonDownCommand = value;
					OnPropertyChanged(() => MouseRightButtonDownCommand);
				}
			}
		}
        public override void OnClick(object o)
        {
            #region Doubleclick detection

            if (LastTicks == 0)
                LastTicks = DateTime.Now.Ticks;
            else
            {
                DeltaTicks = DateTime.Now.Ticks - LastTicks;
                if (DeltaTicks < 2600000)
                {
                    EditorState.DoubleClick = true;
                }
                LastTicks = 0;
            }

            #endregion Doubleclick detection

            NoteController.ViewModel = this;
            NoteController.DispatchTool();
        }

        public void OnMouseRightButtonDown(ExtendedCommandParameter commandParameter)
        {
            EditorState.IsOverNote = true;
            NoteController.SelectedNoteId = Note.Id;
            ChordManager.SelectedChordId = Note.Chord_Id;
        }

        #endregion

        #region Event Aggregation

        public void SubscribeEvents()
        {
            EA.GetEvent<ResetNoteActivationState>().Subscribe(OnResetNoteActivationState);
            EA.GetEvent<DeactivateNotes>().Subscribe(OnDeactivateNotes);
            //EA.GetEvent<HideDispositionButtons>().Subscribe(OnHideDispositionButtons);
            EA.GetEvent<SelectNote>().Subscribe(OnSelectNote);
            EA.GetEvent<SetAccidental>().Subscribe(OnSetAccidental);
            EA.GetEvent<DeSelectNote>().Subscribe(OnDeSelectNote);
            EA.GetEvent<ReverseNoteStem>().Subscribe(OnReverse);
            EA.GetEvent<RejectChange>().Subscribe(OnRejectChange);
            EA.GetEvent<AcceptChange>().Subscribe(OnAcceptChange);
            EA.GetEvent<UpdateNote>().Subscribe(OnUpdateNote);

            EA.GetEvent<UpdateNoteDuration>().Subscribe(OnUpdateNoteDuration);
            EA.GetEvent<SelectComposition>().Subscribe(OnSelectComposition);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
        }

        public void OnSelectComposition(object obj)
        {
            EA.GetEvent<SelectNote>().Publish(Note.Id);
        }

        public void OnReverse(Note nT)
        {
            if (!NoteController.IsRest(nT))
            {
                if (Note.Id == nT.Id)
                {
                    Note.Orientation = (Note.Orientation == (short)_Enum.Orientation.Up) ?
                        (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
                }
            }
        }

        public void OnResetNoteActivationState(object obj)
        {
            if (Note.Type % Defaults.Deactivator == 0)
            {
                Note.Type = (short)(Note.Type / Defaults.Deactivator);
            }
            else if (Note.Type % Defaults.Activator == 0)
            {
                Note.Type = (short)(Note.Type / Defaults.Activator);
            }
        }
        public void OnDeactivateNotes(object obj)
        {
            if (Note.Type % Defaults.Activator == 0)
            {
                Note.Type = (short)(Note.Type / Defaults.Activator);
            }
            if (Note.Type % Defaults.Deactivator == 0)
            {
                Note.Type = (short)(Note.Type / Defaults.Deactivator);
            }
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            var state = (TranspositionState)payload.Item2;
            if (Note.Id == payload.Item1)
            {
                Note.Location_Y = state.Location_Y;
                Note.Octave_Id = (short)state.Octave;
                Note.Pitch = state.Pitch;
                Note.Slot = state.Slot;
                Note.Accidental_Id = state.Accidental_Id;
                Note = Note;
            }
        }

        public void OnDeSelectComposition(object obj)
        {
            if (IsSelected)
            {
                EA.GetEvent<DeSelectNote>().Publish(Note.Id);
            }
        }

        public void OnSetAccidental(Tuple<_Enum.Accidental, Note> payload)
        {
            Note note = payload.Item2;
            if (Note.Id != note.Id) return;

            if (NoteController.IsRest(payload.Item2))
                return;

            var accidental = (from a in Infrastructure.Dimensions.Accidentals.AccidentalList where a.Caption.ToLower() == payload.Item1.ToString().ToLower() select a).Single();
            var accidentalName = (accidental.Name == "b") ? "" : accidental.Name;
            note.Pitch = string.Format("{0}{1}", note.Pitch.Substring(0, 2), accidentalName);
            note.Accidental_Id = accidental.Id;
            Note = note;
        }

        public void OnSelectNote(Guid id)
        {
            if (Note.Id == id)
            {
                if (CollaborationManager.IsActive(_note))
                {
                    Selection.AddNote(Note);
                    EA.GetEvent<ShowNoteEditor>().Publish(string.Empty);
                    ShowSelector();
                }
            }
        }

        public void OnDeSelectNote(Guid id)
        {
            if (Note.Id == id)
            {
                Selection.RemoveNote(Note);
                HideSelector();
            }
        }

        public void OnUpdateNoteDuration(Tuple<Guid, decimal> payload)
        {
            if (Note.Id == payload.Item1)
            {
                Note.Duration = payload.Item2;
            }
        }

        public void OnUpdateNote(Note note)
        {
            if (note.Id == Note.Id)
            {
                Note = note;
            }
        }

        public void OnRejectChange(Guid id)
        {
            if (Note.Id == id)
            {
                var currentStatus = Collaborations.GetStatus(Note);
                switch (EditorState.EditContext)
                {
                    case _Enum.EditContext.Authoring:

                        switch (currentStatus)
                        {
                            case (int)_Enum.Status.ContributorAdded:
                                Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorRejectedAdd);
                                Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorRejectedAdd);
                                break;

                            case (int)_Enum.Status.ContributorDeleted:
                                Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorRejectedDelete);
                                Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorRejectedDelete);
                                break;
                        }
                        break;
                    case _Enum.EditContext.Contributing:

                        switch (currentStatus)
                        {
                            case (int)_Enum.Status.AuthorAdded:
                                Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorRejectedAdd);
                                break;

                            case (int)_Enum.Status.AuthorDeleted:
                                Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorRejectedDelete);
                                break;
                        }
                        break;
                }
                EA.GetEvent<UpdateNote>().Publish(Note);
                EA.GetEvent<UpdateSpanManager>().Publish(ParentMeasure.Id);
                EA.GetEvent<SpanMeasure>().Publish(ParentMeasure.Id);
                //DispositionVisibility = Visibility.Collapsed;
            }
        }

        public void OnAcceptChange(Guid id)
        {
            if (Note.Id == id)
            {
                int? sT = Collaborations.GetStatus(Note);
                switch (EditorState.EditContext)
                {
                    case _Enum.EditContext.Authoring:
                        if (sT == (int)_Enum.Status.ContributorAdded)
                        {
                            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorAccepted);
                            Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorAccepted);
                            SetNotegroupContext();
                            var nG = NotegroupManager.GetNotegroup(Note);
                            EA.GetEvent<FlagNotegroup>().Publish(nG);
                        }
                        else
                        {
                            if (sT == (int)_Enum.Status.ContributorDeleted)
                            {
                                AcceptDeletion((int)_Enum.Status.WaitingOnAuthor, (int)_Enum.Status.ContributorDeleted, (short)_Enum.Status.AuthorAccepted, ParentChord);
                                Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
                            }
                        }
                        break;
                    case _Enum.EditContext.Contributing:

                        if (sT == (int)_Enum.Status.AuthorAdded)
                        {
                            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorAccepted);
                            SetNotegroupContext();
                            Notegroup nG = NotegroupManager.GetNotegroup(Note);
                            EA.GetEvent<FlagNotegroup>().Publish(nG);
                        }
                        else
                        {

                            if (sT == (int)_Enum.Status.AuthorDeleted)
                            {
                                AcceptDeletion((int)_Enum.Status.WaitingOnContributor, (int)_Enum.Status.AuthorDeleted, (short)_Enum.Status.ContributorAccepted, ParentChord);
                            }
                        }
                        break;
                }
                EA.GetEvent<UpdateNote>().Publish(Note);
                EA.GetEvent<UpdateSpanManager>().Publish(ParentMeasure.Id);
                EA.GetEvent<SpanMeasure>().Publish(ParentMeasure.Id);
                //DispositionVisibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Ledger

        private void SetLedger()
        {
            ResetLedger();
            if (! NoteController.IsRest(Note))
            {
                if (CollaborationManager.IsActive(Note))
                {
                    int ledgerSize = (Slot.LedgerMap.ContainsKey(Note.Slot)) ? (short)Slot.LedgerMap[Note.Slot] : 0;
                    if (Math.Abs(ledgerSize) > 0)
                    {
                        Ledger = new Ledger { Note_Id = Note.Id, Size = ledgerSize, Location_X = Note.Location_X };
                        RenderLedger();
                    }
                }
            }
        }

        private void ResetLedger()
        {
            const Visibility targetVisibility = Visibility.Collapsed;

            Top5Visibility = targetVisibility;
            Top4Visibility = targetVisibility;
            Top3Visibility = targetVisibility;
            Top2Visibility = targetVisibility;
            Top1Visibility = targetVisibility;

            Bottom1Visibility = targetVisibility;
            Bottom2Visibility = targetVisibility;
            Bottom3Visibility = targetVisibility;
            Bottom4Visibility = targetVisibility;
            Bottom5Visibility = targetVisibility;
        }

        private void RenderLedger()
        {
            var size = Ledger.Size;
            if (size > 0)
            {
                if (size >= 1) Top5Visibility = Visibility.Visible;
                if (size >= 2) Top4Visibility = Visibility.Visible;
                if (size >= 3) Top3Visibility = Visibility.Visible;
                if (size >= 4) Top2Visibility = Visibility.Visible;
                if (size >= 5) Top1Visibility = Visibility.Visible;
            }
            else
            {
                if (size < 0)
                {
                    if (size <= -1) Bottom1Visibility = Visibility.Visible;
                    if (size <= -2) Bottom2Visibility = Visibility.Visible;
                    if (size <= -3) Bottom3Visibility = Visibility.Visible;
                    if (size <= -4) Bottom4Visibility = Visibility.Visible;
                    if (size <= -5) Bottom5Visibility = Visibility.Visible;
                }
            }
        }

        private Ledger _ledger;
        public Ledger Ledger
        {
            get
            {
                return _ledger;
            }
            set
            {
                _ledger = value;
                OnPropertyChanged(() => Ledger);
            }
        }

        private string _margin;
        public string Margin
        {
            get { return _margin; }
            set
            {
                _margin = value;
                OnPropertyChanged(() => Margin);
            }
        }

        private Visibility _top5Visibility = Visibility.Collapsed;
        public Visibility Top5Visibility
        {
            get { return _top5Visibility; }
            set
            {
                _top5Visibility = value;
                OnPropertyChanged(() => Top5Visibility);
            }
        }

        private Visibility _bottom5Visibility = Visibility.Collapsed;
        public Visibility Bottom5Visibility
        {
            get { return _bottom5Visibility; }
            set
            {
                _bottom5Visibility = value;
                OnPropertyChanged(() => Bottom5Visibility);
            }
        }

        private Visibility _top4Visibility = Visibility.Collapsed;
        public Visibility Top4Visibility
        {
            get { return _top4Visibility; }
            set
            {
                _top4Visibility = value;
                OnPropertyChanged(() => Top4Visibility);
            }
        }

        private Visibility _bottom4Visibility = Visibility.Collapsed;
        public Visibility Bottom4Visibility
        {
            get { return _bottom4Visibility; }
            set
            {
                _bottom4Visibility = value;
                OnPropertyChanged(() => Bottom4Visibility);
            }
        }

        private Visibility _top3Visibility = Visibility.Collapsed;
        public Visibility Top3Visibility
        {
            get { return _top3Visibility; }
            set
            {
                _top3Visibility = value;
                OnPropertyChanged(() => Top3Visibility);
            }
        }

        private Visibility _bottom3Visibility = Visibility.Collapsed;
        public Visibility Bottom3Visibility
        {
            get { return _bottom3Visibility; }
            set
            {
                _bottom3Visibility = value;
                OnPropertyChanged(() => Bottom3Visibility);
            }
        }

        private Visibility _top2Visibility = Visibility.Collapsed;
        public Visibility Top2Visibility
        {
            get { return _top2Visibility; }
            set
            {
                _top2Visibility = value;
                OnPropertyChanged(() => Top2Visibility);
            }
        }

        private Visibility _bottom2Visibility = Visibility.Collapsed;
        public Visibility Bottom2Visibility
        {
            get { return _bottom2Visibility; }
            set
            {
                _bottom2Visibility = value;
                OnPropertyChanged(() => Bottom2Visibility);
            }
        }

        private Visibility _top1Visibility = Visibility.Collapsed;
        public Visibility Top1Visibility
        {
            get { return _top1Visibility; }
            set
            {
                _top1Visibility = value;
                OnPropertyChanged(() => Top1Visibility);
            }
        }

        private Visibility _bottom1Visibility = Visibility.Collapsed;
        public Visibility Bottom1Visibility
        {
            get { return _bottom1Visibility; }
            set
            {
                _bottom1Visibility = value;
                OnPropertyChanged(() => Bottom1Visibility);
            }
        }

        #endregion

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}