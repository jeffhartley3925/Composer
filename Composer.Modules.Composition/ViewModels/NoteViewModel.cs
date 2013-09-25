using System;
using System.Collections.Generic;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Events;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Support;
using System.Windows;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class NoteViewModel : BaseViewModel, INoteViewModel
    {
        private static readonly string AcceptBackground = Application.Current.Resources["DarkGreen"].ToString();
        private static readonly string RejectBackground = Application.Current.Resources["DarkRed"].ToString();
        private const string RejectForeground = "#ffffff";
        private const string AcceptForeground = "#ffffff";

        public long LastTicks = 0;

        public long DeltaTicks = 0;

        private _Enum.Disposition _disposition = _Enum.Disposition.Na;
        private _Enum.DispositionLocation _dispositionLocation = _Enum.DispositionLocation.SideVertical;

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

        private Measure _parentMeasure;
        public Measure ParentMeasure
        {
            get
            {
                if (_parentMeasure == null)
                {
                    var c = (from a in Cache.Measures where a.Id == ParentChord.Measure_Id select a);
                    var e = c as List<Measure> ?? c.ToList();
                    if (e.Any())
                    {
                        _parentMeasure = e.SingleOrDefault();
                    }
                }
                return _parentMeasure;
            }
        }

        private Note _note;
        public Note Note
        {
            get { return _note; }
            set
            {
                _note = value;
                PropertiesPanelMargin = (_note.Orientation == (short)_Enum.Direction.Up) ? "-9,33,0,0" : "-9,73,0,0";
                Location = string.Format("{0}, {1}", ParentChord.Location_X, Location_Y);
                _note.Status = (_note.Status) == null ? "0" : _note.Status;
                EA.GetEvent<SetDispositionButtonProperties>().Publish(Note);
                Location_Y = value.Location_Y;
                SetLedger();
                Status = _note.Status;
                OnPropertyChanged(() => Note);
            }
        }

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

        #region Disposition Buttons

        private int _acceptRow;
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

        private int _acceptColumn;
        public int AcceptColumn
        {
            get { return _acceptColumn; }
            set
            {
                _acceptColumn = value;
                OnPropertyChanged(() => AcceptColumn);
            }
        }

        private int _rejectColumn;
        public int RejectColumn
        {
            get { return _rejectColumn; }
            set
            {
                _rejectColumn = value;
                OnPropertyChanged(() => RejectColumn);
            }
        }

        private string _location = "";
        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged(() => Location);
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

        private Visibility _dispositionVisibility = Visibility.Collapsed;
        public Visibility DispositionVisibility
        {
            get { return _dispositionVisibility; }
            set
            {
                _dispositionVisibility = value;
                DispositionButtonWidth = (_dispositionVisibility == Visibility.Collapsed) ? 0 : 29;
                DispositionButtonHeight = (_dispositionVisibility == Visibility.Collapsed) ? 0 : 30;
                OnPropertyChanged(() => DispositionVisibility);
            }
        }

        private double _dispositionScale = .35;
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

        private int _dispositionStrokeThickness = 3;
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

        private void GetDispositionLocation()
        {
            _dispositionLocation = GetDispositionOrientation();
            SetDispositionMargin();
            ArrangeDispositionButtons();
        }

        private _Enum.DispositionLocation GetDispositionOrientation()
        {
            if (ParentChord.Notes.Count() > 1)
            {
                return _Enum.DispositionLocation.SideHorizontal;
            }
            return _Enum.DispositionLocation.SideVertical;
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

        #endregion

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

        private Visibility _propertiesPanelVisibility = Visibility.Visible;
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
                if (value != _locationY)
                {
                    _locationY = value;
                    OnPropertyChanged(() => Location_Y);
                }
            }
        }

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
                            GetDispositionLocation();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        public void OnSelectComposition(object obj)
        {
            EA.GetEvent<SelectNote>().Publish(Note.Id);
        }

        public void OnReverse(Note note)
        {
            if (!NoteController.IsRest(note))
            {
                if (Note.Id == note.Id)
                {
                    Note.Orientation = (Note.Orientation == (short)_Enum.Orientation.Up) ?
                       (short)_Enum.Orientation.Down : (short)_Enum.Orientation.Up;
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

        public override bool ShowSelector()
        {
            return base.ShowSelector();
        }

        public override bool HideSelector()
        {
            return base.HideSelector();
        }

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

        public void SubscribeEvents()
        {
            EA.GetEvent<ShowDispositionButtons>().Subscribe(OnShowDispositionButtons);
            EA.GetEvent<HideDispositionButtons>().Subscribe(OnHideDispositionButtons);
            EA.GetEvent<SelectNote>().Subscribe(OnSelectNote);
            EA.GetEvent<SetAccidental>().Subscribe(OnSetAccidental);
            EA.GetEvent<DeSelectNote>().Subscribe(OnDeSelectNote);
            EA.GetEvent<ReverseNoteStem>().Subscribe(OnReverse);
            EA.GetEvent<RejectChange>().Subscribe(OnRejectChange);
            EA.GetEvent<AcceptChange>().Subscribe(OnAcceptChange);
            EA.GetEvent<UpdateNote>().Subscribe(OnUpdateNote);
            EA.GetEvent<SetDispositionButtonProperties>().Subscribe(OnSetDispositionButtonProperties);
            EA.GetEvent<AcceptClick>().Subscribe(OnClickAccept);
            EA.GetEvent<RejectClick>().Subscribe(OnClickReject);
            EA.GetEvent<UpdateNoteDuration>().Subscribe(OnUpdateNoteDuration);
            EA.GetEvent<SelectComposition>().Subscribe(OnSelectComposition);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
        }

        public void OnShowDispositionButtons(Guid id)
        {
            if (Note.Id == id)
            {
                DispositionVisibility = Visibility.Visible;
            }
        }

        public void OnHideDispositionButtons(object obj)
        {
            DispositionVisibility = Visibility.Collapsed;
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
            //when 2 ns are tied, their d changes. that's what's happening here
            if (Note.Id == payload.Item1)
            {
                Note.Duration = payload.Item2;
            }
        }

        public void OnSetDispositionButtonProperties(Note note)
        {
            if (note.Id == Note.Id)
            {
                if (CollaborationManager.IsPendingDelete(Collaborations.GetStatus(note)))
                {
                    if (Collaborations.CurrentCollaborator != null)
                    {
                        if (note.Audit.CollaboratorIndex == -1 ||
                            note.Audit.CollaboratorIndex == Collaborations.CurrentCollaborator.Index)
                        {
                            EA.GetEvent<ShowDispositionButtons>().Publish(Note.Id);
                            note.Foreground = Preferences.DeletedColor;
                        }
                    }
                }
                else
                {
                    if ((CollaborationManager.IsPendingAdd(Collaborations.GetStatus(note))))
                    {
                        EA.GetEvent<ShowDispositionButtons>().Publish(Note.Id);
                        note.Foreground = Preferences.AddedColor;
                    }
                    else
                    {
                        EA.GetEvent<HideDispositionButtons>().Publish(string.Empty);
                        note.Foreground = Preferences.NoteForeground;
                    }
                }
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
                EA.GetEvent<SpanMeasure>().Publish(ParentMeasure);
                DispositionVisibility = Visibility.Collapsed;
            }
        }

        private void SetNotegroupContext()
        {
            NotegroupManager.ChordStarttimes = null;
            NotegroupManager.ChordNotegroups = null;
            NotegroupManager.Measure = ParentMeasure;
            NotegroupManager.Chord = ParentChord;
            NotegroupManager.SetMeasureChordNotegroups();
        }

        public void OnAcceptChange(Guid id)
        {
            if (Note.Id == id)
            {
                int currentStatus = Collaborations.GetStatus(Note);
                switch (EditorState.EditContext)
                {
                    case _Enum.EditContext.Authoring:
                        if (currentStatus == (int)_Enum.Status.ContributorAdded)
                        {
                            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.AuthorAccepted);
                            Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.AuthorAccepted);
                            SetNotegroupContext();
                            Notegroup notegroup = NotegroupManager.GetNotegroup(Note);
                            EA.GetEvent<FlagNotegroup>().Publish(notegroup);
                        }
                        else
                        {
                            if (currentStatus == (int)_Enum.Status.ContributorDeleted)
                            {
                                AcceptDeletion((int)_Enum.Status.WaitingOnAuthor, (int)_Enum.Status.ContributorDeleted, (short)_Enum.Status.AuthorAccepted, ParentChord);
                                Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
                            }
                        }
                        break;
                    case _Enum.EditContext.Contributing:

                        if (currentStatus == (int)_Enum.Status.AuthorAdded)
                        {
                            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.ContributorAccepted);
                            SetNotegroupContext();
                            Notegroup notegroup = NotegroupManager.GetNotegroup(Note);
                            EA.GetEvent<FlagNotegroup>().Publish(notegroup);
                        }
                        else
                        {
                            if (currentStatus == (int)_Enum.Status.AuthorDeleted)
                            {
                                AcceptDeletion((int)_Enum.Status.WaitingOnContributor, (int)_Enum.Status.AuthorDeleted, (short)_Enum.Status.ContributorAccepted, ParentChord);
                                
                            }
                        }
                        break;
                }
                EA.GetEvent<UpdateNote>().Publish(Note);
                EA.GetEvent<UpdateSpanManager>().Publish(ParentMeasure.Id);
                EA.GetEvent<SpanMeasure>().Publish(ParentMeasure);
                DispositionVisibility = Visibility.Collapsed;
            }
        }

        private bool ContainsStatus(Note note, _Enum.Status status)
        {
            return Collaborations.GetStatus(note) == (short)status;
        }

        private bool ContainsStatus(Chord chord, _Enum.Status status)
        {
            return chord.Notes.Any(note => ContainsStatus(note, status));
        }

        private void AcceptDeletion(int limboStatus, int deletedStatus, short acceptedStatus, Chord chord)
        {
            //either the author is accepting a contributor deletion, or a contributor is accepting a author deletion. either way,
            //the n is forever gone for both contributor and author. Note: Contributor status is set to Purged here, and 
            //Author status is set to Purged at the end of this method.
            Note.Status = Collaborations.SetStatus(Note, (int)_Enum.Status.Purged);

            //If this was the last n of the ch when it was deleted, then there will be
			//a n that is not visible, but needs to be made visible.
            var r = (from a in Cache.Notes
                     where
                        Collaborations.GetStatus(a) == limboStatus && // only rests can have a Limbo status
                        a.StartTime == Note.StartTime
                     select a);

            var e = r as List<Note> ?? r.ToList();
            if (e.Any())
            {
                var rest = e.SingleOrDefault();
                if (rest != null)
                {
                    //yes, there is a n, but that doesn't mean we can show the n.
					//first check if there are other deleted ns pending accept/reject in this ch?
                    var n = (from a in Cache.Notes 
                              where
                                Collaborations.GetStatus(a) == deletedStatus &&
                                a.StartTime == Note.StartTime
                              select a);

                    if (!n.Any() && !CollaborationManager.IsActive(chord)) //this seems to be a double check. each side of the boolean expression implies the other.
                    {
                        rest.Status = Collaborations.SetStatus(rest, acceptedStatus);
                        rest.Status = Collaborations.SetAuthorStatus(rest, (int)_Enum.Status.AuthorOriginal);
                    }
                }
            }
            Note.Status = Collaborations.SetAuthorStatus(Note, (int)_Enum.Status.Purged);
        }

        public void DefineCommands()
        {
            MouseEnterCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnter, null);
            MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, null);

            MouseLeftButtonDownAcceptCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownAccept, null);

            MouseLeftButtonDownRejectCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownReject, null);
            ClickCommand = new DelegatedCommand<object>(OnClick);
            MouseRightButtonDownCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseRightButtonDown, null);

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

        public void OnMouseRightButtonDown(ExtendedCommandParameter commandParameter)
        {
            EditorState.IsOverNote = true;
            NoteController.SelectedNoteId = Note.Id;
            ChordManager.SelectedChordId = Note.Chord_Id;
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
    }
}