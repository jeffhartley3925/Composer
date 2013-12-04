using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Modules.Composition.Views;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Measure = Composer.Repository.DataService.Measure;
using Selection = Composer.Infrastructure.Support.Selection;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class MeasureViewModel : BaseViewModel, IMeasureViewModel
    {
        public MeasureView View;

        #region Fields

        private decimal[] _chordStartTimes;
        private decimal[] _chordInactiveTimes;
        private decimal Starttime;
        private ObservableCollection<Chord> _activeChords;
        private string _addNoteToChordPath = string.Empty;
        private string _background = Preferences.MeasureBackground;
        private string _barBackground = Preferences.BarBackground;
        private string _barForeground = Preferences.BarForeground;
        private short _barId;
        private string _barMargin = "0";
        private double _baseRatio;
        private Chord _chord;
        private bool _debugging = false;
        private decimal _duration;
        private string _firstName;
        private string _foreground = Preferences.MeasureForeground;
        private string _imageUrl;
        private double _initializedWidth;
        private string _insertNotePath = string.Empty;
        private string _insertRestPath = string.Empty;
        private bool _isMouseCaptured;
        private string _lastName;
        private int _loadedChordsCount;
        private ObservableCollection<LocalSpan> _localSpans;
        private Measure _measure;
        private double _measureBarAfterDragX;
        private double _measureBarBeforeDragX;
        private Dictionary<decimal, List<Notegroup>> _measureChordNotegroups;
        private double _mouseX;
        private bool _okToResize = true;
        private Visibility _playbackControlVisibility = Visibility.Collapsed;
        private double _ratio;
        private string _replaceNoteWithRestPath = string.Empty;
        private string _replaceRestWithNotePath = string.Empty;
        private DataServiceRepository<Repository.DataService.Composition> _repository;
        private ObservableCollection<Verse> _subVerses;
        public static List<Notegroup> ChordNotegroups { get; set; }
        public static Chord Chord { get; set; }
        private Chord _chord1;
        private Chord _chord2;
        public Dictionary<decimal, List<Notegroup>> MeasureChordNotegroups;
        public decimal[] ChordStartTimes;
        public decimal[] ChordInactiveTimes;

        #endregion

        public MeasureViewModel(string id)
        {
            View = null;
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            SubscribeEvents();
            DefineCommands();
            HideSelector();
            var guid = Guid.Parse(id);
            var measure = (from a in Cache.Measures where a.Id == guid select a).DefaultIfEmpty(null).Single();
            if (measure != null)
            {
                Measure = measure;
                Width = int.Parse(Measure.Width);
                if (Measure.TimeSignature_Id != null) TimeSignature_Id = (int)Measure.TimeSignature_Id;
            }
            _initializedWidth = Width;
            UpdateActiveChords();
            UpdateMeasureDuration();
            SetActiveMeasureCount();
            _ratio = GetRatio();
            _baseRatio = _ratio;
            EA.GetEvent<SetMeasureEndBar>().Publish(string.Empty);
            PlaybackControlVisibility = Visibility.Collapsed;
            SetTextPaths();
        }

        public Visibility PlaybackControlVisibility
        {
            get { return _playbackControlVisibility; }
            set
            {
                _playbackControlVisibility = value;
                OnPropertyChanged(() => PlaybackControlVisibility);
            }
        }

        public ObservableCollection<Chord> ActiveChords
        {
            get { return _activeChords ?? (_activeChords = new ObservableCollection<Chord>()); }
            set
            {
                _activeChords = value;
                _activeChords = new ObservableCollection<Chord>(_activeChords.OrderBy(p => p.StartTime));
                OnPropertyChanged(() => ActiveChords);
            }
        }

        public decimal Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged(() => Duration);
            }
        }

        public string BarBackground
        {
            get { return _barBackground; }
            set
            {
                _barBackground = value;
                OnPropertyChanged(() => BarBackground);
            }
        }

        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                OnPropertyChanged(() => ImageUrl);
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                OnPropertyChanged(() => FirstName);
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged(() => LastName);
            }
        }

        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Background);
            }
        }

        public string BarForeground
        {
            get { return _barForeground; }
            set
            {
                _barForeground = value;
                OnPropertyChanged(() => BarForeground);
            }
        }

        public string BarMargin
        {
            get { return _barMargin; }
            set
            {
                _barMargin = value;
                OnPropertyChanged(() => BarMargin);
            }
        }

        public ObservableCollection<Verse> SubVerses
        {
            get { return _subVerses; }
            set
            {
                _subVerses = value;
                OnPropertyChanged(() => SubVerses);
            }
        }

        private int _timeSignatureId;

        public int TimeSignature_Id
        {
            get { return _timeSignatureId; }
            set
            {
                _timeSignatureId = value;
                OnPropertyChanged(() => TimeSignature_Id);

                var timeSignature = (from a in TimeSignatures.TimeSignatureList
                                     where a.Id == _timeSignatureId
                                     select a.Name).First();

                if (string.IsNullOrEmpty(timeSignature))
                {
                    timeSignature =
                        (from a in TimeSignatures.TimeSignatureList
                         where a.Id == Preferences.DefaultTimeSignatureId
                         select a.Name).First();
                }

                DurationManager.Bpm = Int32.Parse(timeSignature.Split(',')[0]);
                DurationManager.BeatUnit = Int32.Parse(timeSignature.Split(',')[1]);
                DurationManager.Initialize();
                Starttime = (Measure.Index) * DurationManager.Bpm;
            }
        }

        public Measure Measure
        {
            get { return _measure; }
            set
            {
                _measure = value;
                Background = Preferences.MeasureBackground;
                BarId = Measure.Bar_Id;
                EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
                Duration = _measure.Duration;
                OnPropertyChanged(() => Measure);
            }
        }

        public short BarId
        {
            //TODO: BarId appears to be unused
            get { return _barId; }
            set
            {
                _barId = value;
                Measure.Bar_Id = _barId;
                BarMargin = (from a in Bars.BarList where a.Id == _barId select a.Margin).First();
                OnPropertyChanged(() => BarId);
            }
        }

        public ObservableCollection<LocalSpan> LocalSpans
        {
            get { return _localSpans; }
            set
            {
                if (value != _localSpans)
                {
                    _localSpans = value;
                    SpanManager.LocalSpans = value;
                    OnPropertyChanged(() => LocalSpans);
                }
            }
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<ArrangeMeasure>().Subscribe(OnArrangeMeasure);
            EA.GetEvent<AdjustAppendSpace>().Subscribe(OnAdjustAppendSpace);
            EA.GetEvent<UpdateActiveChords>().Subscribe(OnUpdateActiveChords);
            EA.GetEvent<NotifyActiveChords>().Subscribe(OnNotifyActiveChords);
            EA.GetEvent<UpdateMeasureBarX>().Subscribe(OnUpdateMeasureBarX);
            EA.GetEvent<HideMeasureEditHelpers>().Subscribe(OnHideMeasureEditHelpers);
            EA.GetEvent<SetPlaybackControlVisibility>().Subscribe(OnSetPlaybackControlVisibility);
            EA.GetEvent<UpdateMeasureBarColor>().Subscribe(OnUpdateMeasureBarColor);
            EA.GetEvent<UpdateMeasureBar>().Subscribe(OnUpdateMeasureBar);
            EA.GetEvent<Backspace>().Subscribe(OnBackspace);
            EA.GetEvent<DeleteTrailingRests>().Subscribe(OnDeleteTrailingRests);
            EA.GetEvent<DeleteEntireChord>().Subscribe(OnDeleteEntireChord);
            EA.GetEvent<ShowMeasureFooter>().Subscribe(OnShowFooter);
            EA.GetEvent<HideMeasureFooter>().Subscribe(OnHideFooter);
            EA.GetEvent<SetMeasureBackground>().Subscribe(OnSetMeasureBackground);
            EA.GetEvent<UpdateSpanManager>().Subscribe(OnUpdateSpanManager);
            EA.GetEvent<SpanUpdate>().Subscribe(OnSpanUpdate);
            EA.GetEvent<ResizeMeasure>().Subscribe(OnResizeMeasure, true);
            EA.GetEvent<MeasureLoaded>().Subscribe(OnMeasureLoaded);
            EA.GetEvent<NotifyChord>().Subscribe(OnNotifyChord);
            EA.GetEvent<ResetMeasureFooter>().Subscribe(OnResetMeasureFooter);
            EA.GetEvent<SelectMeasure>().Subscribe(OnSelectMeasure);
            EA.GetEvent<DeSelectMeasure>().Subscribe(OnDeSelectMeasure);
            EA.GetEvent<ApplyVerse>().Subscribe(OnApplyVerse);
            EA.GetEvent<ClearVerses>().Subscribe(OnClearVerses);
            EA.GetEvent<BroadcastNewMeasureRequest>().Subscribe(OnBroadcastNewMeasureRequest);
            EA.GetEvent<AdjustMeasureWidth>().Subscribe(OnAdjustMeasureWidth);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
            EA.GetEvent<PopEditPopupMenu>().Subscribe(OnPopEditPopupMenu, true);
            EA.GetEvent<UpdateMeasureBar>().Subscribe(OnUpdateMeasureBar);
            EA.GetEvent<SetMeasureEndBar>().Subscribe(OnSetMeasureEndBar);
            EA.GetEvent<ShowSavePanel>().Subscribe(OnShowSavePanel, true);
            EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
            EA.GetEvent<DeselectAllBars>().Subscribe(OnDeselectAllBars);
        }

        public void DefineCommands()
        {
            ClickCommand = new DelegatedCommand<object>(OnClick);
            MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, null);

            MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, null);

            MouseLeaveBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeaveBar, null);
            MouseEnterBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterBar, null);
            MouseLeftButtonUpBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(
                OnMouseLeftButtonUpOnBar, null);
            MouseLeftButtonDownBarCommand =
                new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnBar, null);
            MouseRightButtonUpCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseRightButtonUp, null);
            MouseMoveBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMoveBar, null);

            ClickFooterAcceptAllCommand = new DelegatedCommand<object>(OnClickFooterAcceptAll);
            ClickFooterRejectAllCommand = new DelegatedCommand<object>(OnClickFooterRejectAll);
            ClickFooterCompareCommand = new DelegatedCommand<object>(OnClickFooterCompare);
            ClickFooterSelectAllCommand = new DelegatedCommand<object>(OnClickFooterSelectAll);
            ClickFooterDeleteCommand = new DelegatedCommand<object>(OnClickFooterDelete);
        }

        #region Footer Properties, Commands and EventHandlers

        private Visibility _barVisibility = Visibility.Visible;
        private Visibility _chordSelectorVisiblity = Visibility.Collapsed;

        private DelegatedCommand<object> _clickFooterAcceptAllCommand;
        private DelegatedCommand<object> _clickFooterCompareCommand;
        private DelegatedCommand<object> _clickFooterDeleteCommand;
        private DelegatedCommand<object> _clickFooterPickCommand;

        private DelegatedCommand<object> _clickFooterRejectAllCommand;
        private DelegatedCommand<object> _clickFooterSelectAllCommand;
        private Visibility _collaborationFooterVisible = Visibility.Collapsed;
        private Visibility _editingFooterVisible = Visibility.Collapsed;
        private string _footerSelectAllText = "Select";
        private Visibility _footerSelectAllVisibility = Visibility.Collapsed;
        private Visibility _insertMarkerVisiblity = Visibility.Collapsed;

        public Visibility BarVisibility
        {
            get { return _barVisibility; }
            set
            {
                _barVisibility = value;
                OnPropertyChanged(() => BarVisibility);
            }
        }

        public DelegatedCommand<object> ClickFooterAcceptAllCommand
        {
            get { return _clickFooterAcceptAllCommand; }
            set
            {
                _clickFooterAcceptAllCommand = value;
                OnPropertyChanged(() => ClickFooterAcceptAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterRejectAllCommand
        {
            get { return _clickFooterRejectAllCommand; }
            set
            {
                _clickFooterRejectAllCommand = value;
                OnPropertyChanged(() => ClickFooterRejectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterCompareCommand
        {
            get { return _clickFooterCompareCommand; }
            set
            {
                _clickFooterCompareCommand = value;
                OnPropertyChanged(() => ClickFooterCompareCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterPickCommand
        {
            get { return _clickFooterPickCommand; }
            set
            {
                _clickFooterPickCommand = value;
                OnPropertyChanged(() => ClickFooterPickCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterSelectAllCommand
        {
            get { return _clickFooterSelectAllCommand; }
            set
            {
                _clickFooterSelectAllCommand = value;
                OnPropertyChanged(() => ClickFooterSelectAllCommand);
            }
        }

        public DelegatedCommand<object> ClickFooterDeleteCommand
        {
            get { return _clickFooterDeleteCommand; }
            set
            {
                _clickFooterDeleteCommand = value;
                OnPropertyChanged(() => ClickFooterDeleteCommand);
            }
        }

        public string FooterSelectAllText
        {
            get { return _footerSelectAllText; }
            set
            {
                _footerSelectAllText = value;
                OnPropertyChanged(() => FooterSelectAllText);
            }
        }

        public Visibility FooterSelectAllVisibility
        {
            get { return _footerSelectAllVisibility; }
            set
            {
                _footerSelectAllVisibility = value;
                OnPropertyChanged(() => FooterSelectAllVisibility);
            }
        }

        public Visibility ChordSelectorVisibility
        {
            get { return _chordSelectorVisiblity; }
            set
            {
                _chordSelectorVisiblity = value;
                OnPropertyChanged(() => ChordSelectorVisibility);
            }
        }

        public Visibility InsertMarkerVisiblity
        {
            get { return _insertMarkerVisiblity; }
            set
            {
                _insertMarkerVisiblity = value;
                OnPropertyChanged(() => InsertMarkerVisiblity);
            }
        }

        public Visibility CollaborationFooterVisible
        {
            get { return _collaborationFooterVisible; }
            set
            {
                _collaborationFooterVisible = value;
                OnPropertyChanged(() => CollaborationFooterVisible);
            }
        }

        public Visibility EditingFooterVisible
        {
            get { return _editingFooterVisible; }
            set
            {
                _editingFooterVisible = value;
                OnPropertyChanged(() => EditingFooterVisible);
            }
        }

        private void AcceptOrRejectAll(_Enum.Disposition disposition)
        {
            foreach (var chord in ActiveChords)
            {
                foreach (var note in chord.Notes)
                {
                    switch (disposition)
                    {
                        case _Enum.Disposition.Accept:
                            EA.GetEvent<AcceptChange>().Publish(note.Id);
                            break;
                        case _Enum.Disposition.Reject:
                            EA.GetEvent<RejectChange>().Publish(note.Id);
                            break;
                    }
                }
            }
        }

        public void OnClickFooterAcceptAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            AcceptOrRejectAll(_Enum.Disposition.Accept);
        }

        public void OnClickFooterRejectAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            AcceptOrRejectAll(_Enum.Disposition.Reject);
        }

        public void OnClickFooterCompare(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
        }

        private void DeleteAll()
        {
            var chords = (from a in Measure.Chords select a).ToList();
            foreach (var chord in chords)
            {
                var notes = (from b in chord.Notes select b).ToList();
                foreach (var note in notes)
                {
                    EA.GetEvent<DeleteNote>().Publish(note);
                }
            }
        }

        public void OnResetMeasureFooter(object obj)
        {
            FooterSelectAllText = "Select";
            FooterSelectAllVisibility = Visibility.Collapsed;
        }

        public void OnClickFooterSelectAll(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            if (FooterSelectAllVisibility == Visibility.Visible)
            {
                EA.GetEvent<DeSelectMeasure>().Publish(Measure.Id);
            }
            else
            {
                EA.GetEvent<SelectMeasure>().Publish(Measure.Id);
            }
        }

        public void OnClickFooterDelete(object obj)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            DeleteAll();
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
            HideFooters();
        }

        public void OnHideFooter(Guid id)
        {
            if (id == Measure.Id)
            {
                EditingFooterVisible = Visibility.Collapsed;
                PlaybackControlVisibility = Visibility.Collapsed;
            }
        }

        public void OnShowFooter(_Enum.MeasureFooter footer)
        {
            HideFooters();
            if (Measure.Chords.Count > 0)
            {
                switch (footer)
                {
                    case _Enum.MeasureFooter.Collaboration:
                        if (Measure.Chords.Count - ActiveChords.Count > 0)
                        {
                            CollaborationFooterVisible = Visibility.Visible;
                        }
                        break;
                    case _Enum.MeasureFooter.Editing:
                        var mStaff = Utils.GetStaff(Measure.Staff_Id);
                        if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                            (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && mStaff.Index % 2 == 0))
                        {
                            EditingFooterVisible = Visibility.Visible;
                        }
                        break;
                }
            }
        }

        private void HideFooters()
        {
            EditingFooterVisible = Visibility.Collapsed;
            CollaborationFooterVisible = Visibility.Collapsed;
        }

        #endregion

        #region MeasureBar Methods, Properties, Commands and EventHandlers

        private string _bottomChordSelectorMargin;
        private string _bottomInsertMarkerMargin;
        private string _insertMarkerColor = string.Empty;
        private string _insertMarkerLabelPath = string.Empty;
        private string _markerColor = string.Empty;
        private string _markerLabelPath = string.Empty;
        private int _measureBarX;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeaveBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseRightButtonUpCommand;
        private string _topInsertMarkerLabelMargin;
        private string _topInsertMarkerMargin;
        private string _topMarkerLabelMargin;
        private string _topMarkerMargin;
        private string _verseMargin;
        private int _width;

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeaveBarCommand
        {
            get { return _mouseLeaveBarCommand; }
            set
            {
                _mouseLeaveBarCommand = value;
                OnPropertyChanged(() => MouseLeaveBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterBarCommand
        {
            get { return _mouseEnterBarCommand; }
            set
            {
                _mouseEnterBarCommand = value;
                OnPropertyChanged(() => MouseEnterBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpBarCommand
        {
            get { return _mouseLeftButtonUpBarCommand; }
            set
            {
                _mouseLeftButtonUpBarCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownBarCommand
        {
            get { return _mouseLeftButtonDownBarCommand; }
            set
            {
                _mouseLeftButtonDownBarCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseRightButtonUpCommand
        {
            get { return _mouseRightButtonUpCommand; }
            set
            {
                _mouseRightButtonUpCommand = value;
                OnPropertyChanged(() => MouseRightButtonUpCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveBarCommand
        {
            get { return _mouseMoveBarCommand; }
            set
            {
                _mouseMoveBarCommand = value;
                OnPropertyChanged(() => MouseMoveBarCommand);
            }
        }

        public int MeasureBarX
        {
            get { return _measureBarX; }
            set
            {
                _measureBarX = value;
                OnPropertyChanged(() => MeasureBarX);
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                if (EditorState.IsResizingMeasure)
                {
                    // it was possible to drag a bar to the left of the preceding bar which produced a negative width.
                    if (_width < 40) _width = 40;
                }
                if (EditorState.IsOpening)
                {
                    var w = (from a in Cache.Measures where a.Sequence == Measure.Sequence select double.Parse(a.Width)).Max();
                    _width = (int)w;
                }
                Measure.Width = _width.ToString(CultureInfo.InvariantCulture);
                //TODO: No longer using MeasureBar_X.
                MeasureBarX = 0;
                OnPropertyChanged(() => Width);
            }
        }

        public string VerseMargin
        {
            get { return _verseMargin; }
            set
            {
                _verseMargin = value;
                OnPropertyChanged(() => VerseMargin);
            }
        }

        public string TopMarkerMargin
        {
            get { return _topMarkerMargin; }
            set
            {
                _topMarkerMargin = value;
                OnPropertyChanged(() => TopMarkerMargin);
            }
        }

        public string TopInsertMarkerMargin
        {
            get { return _topInsertMarkerMargin; }
            set
            {
                _topInsertMarkerMargin = value;
                OnPropertyChanged(() => TopInsertMarkerMargin);
            }
        }

        public string TopMarkerLabelMargin
        {
            get { return _topMarkerLabelMargin; }
            set
            {
                _topMarkerLabelMargin = value;
                OnPropertyChanged(() => TopMarkerLabelMargin);
            }
        }

        public string TopInsertMarkerLabelMargin
        {
            get { return _topInsertMarkerLabelMargin; }
            set
            {
                _topInsertMarkerLabelMargin = value;
                OnPropertyChanged(() => TopInsertMarkerLabelMargin);
            }
        }

        public string BottomMarkerMargin
        {
            get { return _bottomChordSelectorMargin; }
            set
            {
                _bottomChordSelectorMargin = value;
                OnPropertyChanged(() => BottomMarkerMargin);
            }
        }

        public string BottomInsertMarkerMargin
        {
            get { return _bottomInsertMarkerMargin; }
            set
            {
                _bottomInsertMarkerMargin = value;
                OnPropertyChanged(() => BottomInsertMarkerMargin);
            }
        }

        public string MarkerLabelPath
        {
            get { return _markerLabelPath; }
            set
            {
                _markerLabelPath = value;
                OnPropertyChanged(() => MarkerLabelPath);
            }
        }

        public string InsertMarkerLabelPath
        {
            get { return _insertMarkerLabelPath; }
            set
            {
                _insertMarkerLabelPath = value;
                OnPropertyChanged(() => InsertMarkerLabelPath);
            }
        }

        public string MarkerColor
        {
            get { return _markerColor; }
            set
            {
                _markerColor = value;
                OnPropertyChanged(() => MarkerColor);
            }
        }

        public string InsertMarkerColor
        {
            get { return _insertMarkerColor; }
            set
            {
                _insertMarkerColor = value;
                OnPropertyChanged(() => InsertMarkerColor);
            }
        }

        public void OnMouseLeaveBar(ExtendedCommandParameter commandParameter)
        {
            if (!EditorState.IsPrinting)
            {
                var item = (Path)commandParameter.Parameter;
                item.Cursor = Cursors.Hand;
                BarBackground = Preferences.BarBackground;
                BarForeground = Preferences.BarForeground;
                EA.GetEvent<UpdateMeasureBarColor>()
                    .Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarForeground));
                EditorState.IsOverBar = false;
            }
        }

        public void OnMouseEnterBar(ExtendedCommandParameter commandParameter)
        {
            if (!EditorState.IsPrinting)
            {
                var item = (Path)commandParameter.Parameter;
                item.Cursor = Cursors.SizeWE;
                BarBackground = Preferences.BarSelectorColor;
                BarForeground = Preferences.BarSelectorColor;
                EA.GetEvent<UpdateMeasureBarColor>()
                    .Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarSelectorColor));
                EditorState.IsOverBar = true;
            }
        }

        public void OnMouseLeftButtonUpOnBar(ExtendedCommandParameter commandParameter)
        {
            if (!EditorState.IsPrinting)
            {
                var item = (Path)commandParameter.Parameter;
                var args = (MouseEventArgs)commandParameter.EventArgs;
                if (_debugging)
                {
                    item.Stroke = new SolidColorBrush(Colors.Black);
                }
                _mouseX = args.GetPosition(null).X;
                _measureBarAfterDragX = _mouseX;
                _isMouseCaptured = false;
                item.ReleaseMouseCapture();
                _mouseX = -1;
                var mStaffgroup = Utils.GetStaffgroup(Measure);
                var payload =
                    new MeasureWidthChangePayload
                    {
                        Id = Measure.Id,
                        Sequence = Measure.Sequence,
                        Width = Width - (int)(_measureBarBeforeDragX - _measureBarAfterDragX),
                        StaffgroupId = mStaffgroup.Id
                    };

                EA.GetEvent<ResizeMeasure>().Publish(payload);
                _initializedWidth = Width;
                EditorState.IsResizingMeasure = false;
            }
        }

        public void OnMouseLeftButtonDownOnBar(ExtendedCommandParameter commandParameter)
        {
            if (!EditorState.IsPrinting)
            {
                EditorState.IsResizingMeasure = true;
                EA.GetEvent<HideEditPopup>().Publish(string.Empty);
                var item = (Path)commandParameter.Parameter;
                var args = (MouseEventArgs)commandParameter.EventArgs;
                _mouseX = args.GetPosition(null).X;
                _measureBarBeforeDragX = _mouseX;
                _isMouseCaptured = true;
                item.CaptureMouse();
            }
        }

        public void GetNextPasteTarget()
        {
            // if the content of the clipboard is greater than the remaining s'space' in the target _measure, then
            // this method is called to help determine what _measure the paste should continue in.
            int index = Measure.Index;
            var measure =
                (from a in Cache.Measures where a.Index == index + 1 select a).DefaultIfEmpty(null).Single();
            EA.GetEvent<BroadcastNewMeasureRequest>().Publish(measure);
        }

        public void OnPopEditPopupMenu(Guid id)
        {
            if (id == Measure.Id)
            {
                var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset,
                    MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
                // var pt = new Point(MeasureClick_X + 10, MeasureClick_Y + 10);
                var payload =
                    new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index,
                        Measure.Index * DurationManager.Bpm, DurationManager.Bpm, Measure.Width, Measure.Staff_Id);

                EA.GetEvent<SetEditPopupMenu>().Publish(payload);
                EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
                EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Publish(string.Empty);
                EA.GetEvent<ShowEditPopupMenu>().Publish(string.Empty);

                SetChordContext();
            }
        }

        public void OnMouseLeftButtonUpPreviousCollaborator(object obj)
        {
        }

        public void OnMouseLeftButtonUpNextCollaborator(object obj)
        {
        }

        public void OnMouseRightButtonUp(ExtendedCommandParameter commandParameter)
        {
            var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset,
                MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
            var payload =
                new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index,
                    Measure.Index * DurationManager.Bpm, DurationManager.Bpm, Measure.Width, Measure.Staff_Id);

            EA.GetEvent<SetEditPopupMenu>().Publish(payload);
            EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
            EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Publish(string.Empty);
            EA.GetEvent<ShowEditPopupMenu>().Publish(string.Empty);

            SetChordContext();
        }

        public void OnMouseMoveBar(ExtendedCommandParameter commandParameter)
        {
            try
            {
                if (EditorState.IsPrinting) return;
                var item = (Path)commandParameter.Parameter;
                var e = (MouseEventArgs)commandParameter.EventArgs;
                if (!_isMouseCaptured) return;
                BarBackground = Preferences.BarSelectorColor;
                BarForeground = Preferences.BarSelectorColor;
                var x = e.GetPosition(null).X;
                var deltaH = x - _mouseX;
                var newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);
                EA.GetEvent<UpdateMeasureBarX>().Publish(new Tuple<Guid, double>(Measure.Id, Math.Round(newLeft, 0)));
                EA.GetEvent<UpdateMeasureBarColor>().Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarSelectorColor));
                item.SetValue(Canvas.LeftProperty, newLeft);
                _mouseX = e.GetPosition(null).X;
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        public void OnResizeMeasure(object obj)
        {
            var payload = (MeasureWidthChangePayload)obj;
            try
            {
                EditorState.Ratio = 1;
                EditorState.MeasureResizeScope = _Enum.MeasureResizeScope.Composition;
                if (payload.Sequence == _measure.Sequence)
                {
                    SetWidth(payload.Width);
                    AdjustContent();
                }
                EA.GetEvent<DeselectAllBars>().Publish(string.Empty);
                EA.GetEvent<ArrangeVerse>().Publish(Measure);
                EA.GetEvent<ArrangeArcs>().Publish(Measure);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        private void AdjustContent()
        {
            try
            {
                var action = Preferences.MeasureArrangeMode;
                if (ActiveChords.Count > 0)
                {
                    if (MeasureManager.IsPacked(Measure))
                    {
                        Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.ManualResizePacked;
                        EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                        Preferences.MeasureArrangeMode = action;
                    }
                    else
                    {
                        Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.ManualResizeNotPacked;
                        EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                        var chord = (from c in ActiveChords select c).OrderBy(p => p.StartTime).Last();
                        if (chord.Location_X + Preferences.MeasureMaximumEditingSpace > Width)
                        {
                            AdjustTrailingSpace(Preferences.MeasureMaximumEditingSpace);
                        }
                        Preferences.MeasureArrangeMode = action;
                    }
                }
                var mStaff = Utils.GetStaff(_measure.Staff_Id);
                if (mStaff == null) return;
                var w = (from a in mStaff.Measures select double.Parse(a.Width)).Sum() +
                        Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin - 70;
                EditorState.GlobalStaffWidth = w;
                EA.GetEvent<SetProvenanceWidth>().Publish(w);
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        private void SetWidth(double width)
        {
            _ratio = 1;
            if (!EditorState.IsOpening)
            {
                _ratio = width / Width * _baseRatio;
                _baseRatio = _ratio;
            }
            Width = (int)Math.Floor(width);
        }

        public struct MeasureWidthChangePayload
        {
            public Guid Id;
            public int Sequence;
            public Guid StaffgroupId;
            public int Width;

            public MeasureWidthChangePayload(Guid id, int sequence, int width, Guid staffgroupId)
            {
                Id = id;
                Sequence = sequence;
                Width = width;
                StaffgroupId = staffgroupId;
            }
        }

        #endregion

        private void SetChordContext()
        {
            ChordManager.Location_X = MeasureClick_X;
            ChordManager.Location_Y = MeasureClick_Y;
            ChordManager.ChordNotegroups = null;
            ChordManager.Measure = Measure;
        }

        private void SetNotegroupContext()
        {
            NotegroupManager.ChordStarttimes = null;
            NotegroupManager.ChordNotegroups = ChordNotegroups;
            NotegroupManager.Measure = Measure;
            NotegroupManager.Chord = Chord;
        }

        private void UpdateActiveChords()
        {
            // this is the first time IsActionable is called for notes in a loading composition....
            EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
            // ...so, at this point in the flow, every note in the measure has been activated or deactivated.
        }

        private static void SetActiveMeasureCount()
        {
            // why are we excluding the first m - (a.Index > 0 )?
            EditorState.ActiveMeasureCount =
                (from a in Cache.Measures where ChordManager.GetActiveChords(a).Count > 0 && a.Index > 0 select a)
                    .DefaultIfEmpty(null)
                    .Count();
        }

        private void UpdateMeasureDuration()
        {
            Duration = (decimal)Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
        }

        private void SetTextPaths()
        {
            if (EditorState.UseVerboseMouseTrackers)
            {
                _addNoteToChordPath =
                    (from a in Vectors.VectorList where a.Name == "AddNoteToChord" select a.Path).First();
                _insertNotePath = (from a in Vectors.VectorList where a.Name == "InsertNote" select a.Path).First();
                _insertRestPath = (from a in Vectors.VectorList where a.Name == "InsertRest" select a.Path).First();
                _replaceNoteWithRestPath =
                    (from a in Vectors.VectorList where a.Name == "ReplaceNoteWithRest" select a.Path).First();
                _replaceRestWithNotePath =
                    (from a in Vectors.VectorList where a.Name == "ReplaceRestWithNote" select a.Path).First();
            }
            else
            {
                _addNoteToChordPath = (from a in Vectors.VectorList where a.Name == "Add" select a.Path).First();
                _insertNotePath = (from a in Vectors.VectorList where a.Name == "Insert" select a.Path).First();
                _insertRestPath = (from a in Vectors.VectorList where a.Name == "Insert" select a.Path).First();
                _replaceNoteWithRestPath =
                    (from a in Vectors.VectorList where a.Name == "Replace" select a.Path).First();
                _replaceRestWithNotePath =
                    (from a in Vectors.VectorList where a.Name == "Replace" select a.Path).First();
            }
        }

        private double GetRatio()
        {
            double ratio = 1;
            if (EditorState.IsOpening)
            {
                if (ActiveChords.Count <= 1) return ratio;
                var actualProportionalSpacing = ActiveChords[1].Location_X - ActiveChords[0].Location_X;
                double defaultProportionalSpacing =
                    DurationManager.GetProportionalSpace((double)ActiveChords[0].Duration);
                ratio = actualProportionalSpacing / defaultProportionalSpacing;
            }
            else
            {
                ratio = Width / _initializedWidth;
            }
            return ratio;
        }

        public override void OnMouseLeave(ExtendedCommandParameter param)
        {
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            HideCursor();
            HideLedgerGuide();
            HideMarker();
        }

        private void SwitchContext()
        {
            SpanManager.LocalSpans = LocalSpans;
        }

        public override void OnClick(object obj)
        {
            EA.GetEvent<HideEditPopup>().Publish(string.Empty);
            if (Selection.Notes.Any() || Selection.Arcs.Any())
            {
                // there's an active selection, so stop here and use this click to deselect all selected ns
                EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                return;
            }
            // notify the parent staff about the click so the staff can do 
            // whatever it needs to do when a _measure is clicked.
            EA.GetEvent<SendMeasureClickToStaff>().Publish(Measure.Staff_Id);
            // remove active _measure status from all Measures
            EA.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
            // make this m the active _measure
            EA.GetEvent<SetMeasureBackground>().Publish(Measure.Id);

            if (EditorState.DurationSelected())
            {
                // ...the user has clicked on the m with a n or n tool.
                EditorState.Duration = (from a in DurationManager.Durations
                                        where (a.Caption == EditorState.DurationCaption)
                                        select a.Value).DefaultIfEmpty(Constants.INVALID_DURATION).Single();
                if (ValidPlacement())
                {
                    SetChordContext();
                    _chord = AddNoteToChord();
                    // TODO: Why am I updating the provenance panel every time I click a measure?
                    EA.GetEvent<UpdateProvenancePanel>().Publish(CompositionManager.Composition);
                }
                else
                {
                   //EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                }
            }
            else
            {
                // the user clicked with a tool that is not a note or rest. route click to tool dispatcher
                OnToolClick();
            }
            UpdateActiveChords();
            UpdateMeasureDuration();
            SetActiveMeasureCount();
        }

        public void OnAdjustAppendSpace(Guid id)
        {
            // we don't know what the final width of a measure will be, so we give it a reasonable width,
            // then increase the width as needed by making sure the distance between the measure end bar
            // and the last note in the measure never falls below a defined proportional minimum value.
            var m = Measure;
            if (id != Measure.Id) return;

            if (MeasureManager.IsPacked(Measure))
            {
                AdjustTrailingSpace(Preferences.MeasureMaximumEditingSpace);
            }
            else
            {
                if (ActiveChords.Count <= 0) return;
                var ch = ActiveChords.Last();
                if (ch.Location_X + Preferences.MeasureMaximumEditingSpace > Width)
                {
                    AdjustTrailingSpace(Preferences.MeasureMaximumEditingSpace);
                }
            }
        }

        public bool ValidPlacement()
        {
            var result = true;
            try
            {
                var isPackedMeasure = MeasureManager.IsPacked(Measure);
                var isAddingToChord = IsAddingToChord();
                if (EditorState.Duration != Constants.INVALID_DURATION)
                {
                    if (EditorState.Duration == null) return false;
                    result = (!isPackedMeasure || isAddingToChord) &&
                             (Duration + (decimal)EditorState.Duration <= DurationManager.Bpm || isAddingToChord);
                }
            }
            catch (Exception ex)
            {
                result = false;
                Exceptions.HandleException(ex);
            }
            return result;
        }

        public void OnToolClick()
        {
            var p = Utilities.CoordinateSystem.TranslateToCompositionCoords
                (
                    MeasureClick_X,
                    MeasureClick_Y,
                    Measure.Sequence,
                    Measure.Index,
                    (double)Starttime,
                    DurationManager.Bpm,
                    Measure.Width,
                    Measure.Staff_Id
                );

            switch (EditorState.Tool)
            {
                case "SelectArea":
                    EditorState.ClickMode = "Click";
                    EA.GetEvent<AreaSelect>().Publish(p);
                    break;
            }
        }

        public void OnUpdateSpanManager(object obj)
        {
            var id = (Guid)obj;
            if (id == Measure.Id)
            {
                SpanManager.LocalSpans = LocalSpans;
            }
        }

        public void OnSpanUpdate(object obj)
        {
            var payload = (SpanPayload)obj;
            if (payload.Measure.Id == Measure.Id)
            {
                LocalSpans = payload.LocalSpans;
            }
        }

        private bool IsAddingToChord()
        {
            return (EditorState.Chord != null);
        }

        public void OnUpdateActiveChords(Guid id)
        {
            var chs = ActiveChords;
            if (id == Measure.Id && Measure.Chords.Count > 0)
            {
                chs = new ObservableCollection<Chord>((
                    from a in Measure.Chords
                    where CollaborationManager.IsActive(a)
                    select a).OrderBy(p => p.StartTime));
            }
            ActiveChords = (ObservableCollection<Chord>)chs;
            EA.GetEvent<NotifyActiveChords>().Publish(new Tuple<Guid, object>(Measure.Id, chs));
        }

        public void OnNotifyActiveChords(Tuple<Guid, object> payload)
        {
            var id = payload.Item1;
            if (id != Measure.Id) return;
            ActiveChords = (ObservableCollection<Chord>)payload.Item2;
        }

        public void OnUpdateMeasureBarX(Tuple<Guid, double> payload)
        {
            var m = Utils.GetMeasure(payload.Item1);
            if (m.Sequence == Measure.Sequence)
            {
                try
                {
                    MeasureBarX = int.Parse(payload.Item2.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Exceptions.HandleException(ex);
                }
            }
        }

        public void OnUpdateMeasureBarColor(Tuple<Guid, string> payload)
        {
            var m = Utils.GetMeasure(payload.Item1);
            if (m.Sequence == Measure.Sequence)
            {
                BarForeground = payload.Item2;
            }
        }

        public void OnSetPlaybackControlVisibility(Guid id)
        {
            if (id == Measure.Id)
            {
                var mStaff = Utils.GetStaff(Measure.Staff_Id);
                if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                    (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && mStaff.Index % 2 == 0))
                {
                    PlaybackControlVisibility = (Measure.Chords.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public void OnDeselectAllBars(object obj)
        {
            BarForeground = Preferences.BarForeground;
            BarBackground = Preferences.BarBackground;
        }

        public void OnResumeEditing(object obj)
        {
            EditingFooterVisible = Visibility.Visible;
        }

        public void OnShowSavePanel(object obj)
        {
            EditingFooterVisible = Visibility.Collapsed;
        }

        public void OnBackspace(object obj)
        {
            if (Measure.Id == EditorState.ActiveMeasureId)
            {
                var chords = new ObservableCollection<Chord>(ActiveChords.OrderByDescending(p => p.StartTime));
                if (chords.Count > 0)
                {
                    EA.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(Measure.Id, chords[0].Notes[0].Id));
                    SpanManager.LocalSpans = LocalSpans;
                    EA.GetEvent<SpanMeasure>().Publish(Measure);
                    if (chords.Count == 1)
                    {
                        EA.GetEvent<HideMeasureFooter>().Publish(Measure.Id);
                    }
                }
            }
        }

        public void OnSetMeasureBackground(Guid id)
        {
            if (id == Guid.Empty)
            {
                Background = Preferences.MeasureBackground;
                EditorState.ActiveMeasureId = Guid.Empty;
            }
            else if (id == Measure.Id)
            {
                EA.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
                Background = Preferences.ActiveMeasureBackground;
                EditorState.ActiveMeasureId = Measure.Id;
            }
        }

        public void OnSetMeasureEndBar(object obj)
        {
            try
            {
                if (Measure.Sequence != (Densities.MeasureDensity - 1) * Defaults.SequenceIncrement) return;
                var sg = Utils.GetStaffgroup(Measure);
                if (sg.Sequence != (Densities.StaffgroupDensity - 1) * Defaults.SequenceIncrement) return;
                if (Measure.Bar_Id == Bars.StandardBarId)
                {
                    BarId = Bars.EndBarId;
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        public void OnUpdateMeasureBar(short barId)
        {
            // this event is broadcast to all measures. if this m has a end-bar with end-bar id = Bars.EndBarId, (if it 
            // is the last m in the last staffgroup), then it is reset to the bar id passed in.

            if (!EditorState.IsAddingStaffgroup) return;
            if (BarId == Bars.EndBarId)
            {
                BarId = barId;
            }
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            var state = (TranspositionState)payload.Item2;
            Measure.Key_Id = state.Key.Id;
        }

        public void OnAdjustMeasureWidth(Tuple<Guid, double> payload)
        {
            // when a _measure is not packed, but there's no room to add another ch, the
            // AdjustMeasureWidth event is raised.

            var id = payload.Item1;
            var endSpace = payload.Item2;
            if (id != Measure.Id) return;
            if (ActiveChords.Count <= 0) return;
            // set the _measure width to the x coordinate of the last ch in the _measure plus an integer value passed 
            // in via the event payload - usually Preferences.MeasureMaximumEditingSpace * _measure spacing ratio.

            // get the last ch in the m, then...
            var ch = (from c in ActiveChords select c).OrderBy(q => q.StartTime).Last();

            // ...add the calculated (passed in) width to get the new m Width
            var maxWidthInSequence =
                int.Parse((from c in Cache.Measures where c.Sequence == Measure.Sequence select c.Width).Max());
            var proposedWidth = ch.Location_X + (int)Math.Floor(endSpace);

            // the "Width = ch..." line above sets the width of the m. "ResizeMeasure" below also sets the width 
            // of the m, among other things. however, if you comment out the line "Width = ch...." above so that we 
            // are setting the width only once, results are unpredictable. so we are setting the width twice pending a real solution.

            // NOTE: we have to set the width in ResizeMeasure so that the width is broadcast to all measures in the same seq.

            // TODO; the parameter (payload) for AdjustMeasureWidth event should be what it is for the ResizeMeasure event 
            // so that the "if (id == _measure.Id)" test above can become "if (seq == _measure.Sequence)". that way we won't
            // have to call ResizeMeasure since AdjustMeasureWidth will be broadcast to all measures with the same seq just 
            // like ResizeMeasure is.

            if (!_okToResize) return;
            if (proposedWidth > maxWidthInSequence)
            {
                Width = proposedWidth;
                EA.GetEvent<ResizeMeasure>()
                    .Publish(new MeasureWidthChangePayload
                    {
                        Id = Measure.Id,
                        Sequence = Measure.Sequence,
                        Width = proposedWidth,
                        StaffgroupId = Guid.Empty
                    });
            }
            else
            {
                var action = Preferences.MeasureArrangeMode;
                Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.IncreaseMeasureSpacing;
                EditorState.NoteSpacingRatio = maxWidthInSequence / (double)proposedWidth;
                EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                EditorState.NoteSpacingRatio = 1;
                Preferences.MeasureArrangeMode = action;
            }
        }

        private void spatialFinalizer()
        {

        }

        private void SetRepository()
        {
            if (_repository == null)
            {
                _repository =
                    ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }

        public void OnDeleteTrailingRests(object obj)
        {
            SetRepository();
            var nIds = new List<Guid>();
            foreach (var ch in ActiveChords)
            {
                if (ch.Notes[0].Pitch == "R")
                    nIds.Add(ch.Notes[0].Id);
                else
                    break;
            }
            foreach (var nId in nIds)
            {
                EA.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(Measure.Id, nId));
            }
        }

        public void OnDeleteEntireChord(Tuple<Guid, Guid> payload)
        {
            SetRepository();
            if (payload.Item1 != Measure.Id) return;
            var n = Utils.GetNote(payload.Item2);
            if (!CollaborationManager.IsActive(n)) return;
            var ch = Utils.GetChord(n.Chord_Id);
            if (ch == null) return;
            var duration = (from c in ch.Notes select c.Duration).DefaultIfEmpty<decimal>(0).Min();
            DeleteChordNotes(ch);
            RemoveChordFromMeasure(ch, duration);
            AdjustFollowingChords(n, duration);
            UpdateActiveChords();
            UpdateMeasureDuration();
        }

        private void DeleteChordNotes(Chord ch)
        {
            var ids = ch.Notes.Select(n => n.Id).ToList();
            foreach (var id in ids)
            {
                var n = Utils.GetNote(id);
                _repository.Delete(n);
                Cache.Notes.Remove(n);
                ch.Notes.Remove(n);
            }
        }

        private void RemoveChordFromMeasure(Chord ch, decimal chDuration)
        {
            Measure.Chords.Remove(ch);
            _repository.Delete(ch);
            Cache.Chords.Remove(ch);
            Measure.Duration = Math.Max(0, Measure.Duration - chDuration);
        }

        private void AdjustFollowingChords(Note n, decimal chDuration)
        {
            foreach (var ch in ActiveChords)
            {
                if (ch.Location_X <= n.Location_X) continue;
                ch.StartTime = ch.StartTime - (double)chDuration;
                EA.GetEvent<SynchronizeChord>().Publish(ch);
                EA.GetEvent<UpdateChord>().Publish(ch);
            }
        }

        public void OnBroadcastNewMeasureRequest(object obj)
        {
            if (obj == null) return;
            var measure = (Measure)obj;
            if (Measure.Index != measure.Index) return;
            // the next paste target _measure (calculated in GetNextPasteTarget()) is sent to the EditPopupMenu ViewModel.
            EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
            // send 'Paste' command as if it was selected on the Edit Popup Menu (or control-V);
            EA.GetEvent<EditPopupItemClicked>()
                .Publish(new Tuple<string, string, _Enum.PasteCommandSource>(EditActions.Paste, "",
                    _Enum.PasteCommandSource.Programmatic));
        }

        public void OnClearVerses(object obj)
        {
            SubVerses = new ObservableCollection<Verse>();
        }

        public void OnDeSelectMeasure(Guid id)
        {
            if (Measure.Id != id) return;
            FooterSelectAllVisibility = Visibility.Collapsed;
            FooterSelectAllText = "Select";
            foreach (var ch in ActiveChords)
            {
                EA.GetEvent<DeSelectChord>().Publish(ch.Id);
            }
        }

        public void OnSelectMeasure(Guid id)
        {
            if (Measure.Id != id) return;
            FooterSelectAllVisibility = Visibility.Visible;
            FooterSelectAllText = "Deselect";
            foreach (var chord in ActiveChords)
            {
                EA.GetEvent<SelectChord>().Publish(chord.Id);
            }
        }

        public void OnNotifyChord(Guid id)
        {
            // this method determines when the measure is loaded by tracking the number of loaded chords.
            // when the number of loaded chords is = to the number of chords in the m then we publish 
            // MeassureLoaded event, and then unsubscribe. only needed when a composition is loaded.

            if (id != Measure.Id) return;
            _loadedChordsCount++;
            if (_loadedChordsCount != Measure.Chords.Count()) return;
            if (Measure.Chords.Any())
            {
                EA.GetEvent<MeasureLoaded>().Publish(Measure.Id);
            }
            EA.GetEvent<NotifyChord>().Unsubscribe(OnNotifyChord);
        }

        private void DistributeLyrics()
        {
            CompositionManager.Composition.Verses.OrderBy(p => p.Index);
            Cache.Verses = CompositionManager.Composition.Verses;
            EA.GetEvent<UpdateVerseIndexes>().Publish(Cache.Verses.Count);
        }

        private static bool CheckAllActiveMeasuresLoaded()
        {
            EditorState.LoadedActiveMeasureCount++;
            return EditorState.ActiveMeasureCount == EditorState.LoadedActiveMeasureCount;
        }

        public void OnMeasureLoaded(Guid id)
        {
            // some chords in a _measure may not be actionable (inactive), so they aren't visible, and void of meaning. 
            // the side effect  of this is that some information needed to accurately place a chord spatially in the measure may not
            // be known until after all chords in the measure have been loaded. so after the measure is loaded, this
            // event fires, and all chords are examined again, and adjusted (for visibility and/or horizontal location, 
            // or foreground color) if necessary.

            // NOTE: this handler is called in many more situations than originally intended (see comments above for original intent.)
            // it's a fairly substantial effort to re-factor this code.

            // verse numbers appear in the first _measure (index = 1) _measure only. right now, the verse 
            // margin is the same whether notes exist or not. but leave ability to vary margin anyway.
            VerseMargin = "8,-5,0,0";

            // we need to know when a saved composition has finished loading. a surrogate for this can be when the number
            // of loaded measures equals the number of measures in the composition. So, track the number of loaded measures.
            EditorState.RunningLoadedMeasureCount++;

            if (Measure.Id == id)
            {
                if (ActiveChords.Any())
                {
                    EA.GetEvent<SetPlaybackControlVisibility>().Publish(Measure.Id);
                    if (CheckAllActiveMeasuresLoaded())
                    {
                        DistributeLyrics();
                        EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
                        DistributeArcs();
                        EA.GetEvent<ArrangeVerse>().Publish(Measure);
                        EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
                    }
                    AdjustChords();
                    AdjustTrailingSpace();
                    ReSpan();
                }
            }
            if (EditorState.RunningLoadedMeasureCount != Densities.MeasureCount) return;
            SetGlobalStaffWidth(); // TODO: Is this necessary?
            EA.GetEvent<SetSocialChannels>().Publish(string.Empty);
            EA.GetEvent<SetRequestPrompt>().Publish(string.Empty);
            EditorState.IsOpening = false;
        }

        private void SetGlobalStaffWidth()
        {
            var mStaff = Utils.GetStaff(_measure.Staff_Id);
            var mStaffWidth = (from a in mStaff.Measures select double.Parse(a.Width)).Sum() +
                              Defaults.StaffDimensionWidth +
                              Defaults.CompositionLeftMargin - 70;
            EditorState.GlobalStaffWidth = mStaffWidth;
        }

        private void AdjustChords()
        {
            // the actual x coord and starttime of a chord can vary, depending 
            // on the current user, currently selected collaborator, etc. We make those
            // adjusments here.
            decimal[] chordStarttimes;
            decimal[] chordInactiveTimes;
            decimal[] chordActiveTimes;
            var id = Guid.Empty;
            SetNotegroupContext();
            NotegroupManager.ParseMeasure(out chordStarttimes, out chordInactiveTimes, out chordActiveTimes, ActiveChords);
            foreach (var st in chordActiveTimes) // on 10/1/2012 changed chordStarttimes to chordActiveTimes
            {
                foreach (var ch in ActiveChords.Where(chord => chord.StartTime == (double)st))
                {
                    ch.Duration = ChordManager.SetDuration(ch);
                    if (Math.Abs(_ratio) < double.Epsilon) _ratio = GetRatio();
                    var payload = new Tuple<Guid, Guid, double>(ch.Id, id, _ratio);
                    EA.GetEvent<SetChordLocationAndStarttime>().Publish(payload);
                    id = ch.Id;
                    break;
                }
            }
        }

        private void DistributeArcs()
        {
            if (CompositionManager.Composition.Arcs.Count <= 0) return;
            if (EditorState.ArcsLoaded) return;
            EA.GetEvent<BroadcastArcs>().Publish(CompositionManager.Composition.Arcs);
            EditorState.ArcsLoaded = true;
        }

        private void AdjustTrailingSpace()
        {
            if (MeasureManager.IsPacked(Measure))
            {
                // ...then make sure end bar is proportionally spaced after last ch
                _okToResize = false;
                AdjustTrailingSpace(Preferences.MeasureMaximumEditingSpace);
                _okToResize = true;
            }
        }

        private void ReSpan()
        {
            SpanManager.LocalSpans = LocalSpans;
            EA.GetEvent<SpanMeasure>().Publish(Measure);
        }

        private void AdjustTrailingSpace(double defaultEndSpace)
        {
            // we want the space between the last ch and the m end-bar to be proportional to the n spacing.
            // the 'w' passed in is the end spacing that a m of default width would have. if the m has been
            // resized, then 'w' needs to be adjusted proportionally. 

            var proportionallyAdjustedEndSpace = defaultEndSpace * _ratio * _baseRatio;

            // however, for aesthetic reasons, there is a minimum end-space below which we do not want to go 
            // below, and maximum end-space we don't want to go above.

            if (proportionallyAdjustedEndSpace > Preferences.MeasureMaximumEditingSpace)
                proportionallyAdjustedEndSpace = Preferences.MeasureMaximumEditingSpace;
            else if (proportionallyAdjustedEndSpace < Preferences.MeasureMinimumEditingSpace)
                proportionallyAdjustedEndSpace = Preferences.MeasureMinimumEditingSpace;

            // the handler for the AdjustMeasureWidth event will find the x coordinate of the last ch in the m, then 
            // add 'w' to it for the new m width.

            EA.GetEvent<AdjustMeasureWidth>()
                .Publish(new Tuple<Guid, double>(Measure.Id, proportionallyAdjustedEndSpace));
        }

        public void OnApplyVerse(Tuple<object, int, int, Guid, int, int> payload)
        {
            // Repository.DataService.Verse collection is a member of Repository.DataService.Composition, but we don't 
            // bind to this collection because the binding scope needs to be Repository.DataService._measure. IE: the 
            // storage scope of verses is composition level, but the binding Scope is the _measure. I don't want to 
            // spin up temporary sub-collections of Repository.DataService.Verse objects. Instead, spin up a different 
            // collection of Verses to bind too. This design choice also helps facilitate the projection of verse text 
            // into words (we don't persist words as a separate entity, but each word still has a view, view-model, etc)

            var id = payload.Item4;

            if (id == Measure.Id) // is this the measureViewModel for the target measure?
            {
                var words = (ObservableCollection<Word>)payload.Item1;
                var index = payload.Item3;
                var v = new Verse(index, id.ToString())
                {
                    Words = words,
                    VerseText = string.Empty,
                    Disposition = payload.Item5
                };

                if (SubVerses == null)
                    SubVerses = new ObservableCollection<Verse>();

                SubVerses.Add(v);

                var sv = new List<Verse>();
                sv.AddRange(SubVerses.OrderBy(i => i.Index));
                SubVerses = new ObservableCollection<Verse>(sv);

                // verseCount required for trestleHeight calculation;
                EditorState.VerseCount = CompositionManager.Composition.Verses.Count;
                // force bind to new trestleHeight value by setting EmptyBind to anything.
                EmptyBind = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
        }

        #region Visual Helpers

        private CompositionView _compositionView;
        private string _coordinates;
        private Visibility _cursorVisible = Visibility.Collapsed;
        private int _cursorX;
        private int _cursorY;
        private Visibility _ledgerGuideVisible = Visibility.Collapsed;
        private int _ledgerGuideX;
        private int _ledgerGuideY;
        private int _measureClickX;
        private int _measureClickY;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveCommand;

        public int MeasureClick_Y
        {
            get { return _measureClickY; }
            set
            {
                _measureClickY = value;
                OnPropertyChanged(() => MeasureClick_Y);
            }
        }

        public int MeasureClick_X
        {
            get { return _measureClickX; }
            set
            {
                _measureClickX = value;
                OnPropertyChanged(() => MeasureClick_X);
            }
        }

        public double CompositionClickX { get; set; }
        public double CompositionClickY { get; set; }

        public int LedgerGuide_X
        {
            get { return _ledgerGuideX; }
            set
            {
                _ledgerGuideX = value;
                OnPropertyChanged(() => LedgerGuide_X);
            }
        }

        public int LedgerGuide_Y
        {
            get { return _ledgerGuideY; }
            set
            {
                _ledgerGuideY = value;
                OnPropertyChanged(() => LedgerGuide_Y);
            }
        }

        public Visibility LedgerGuideVisible
        {
            get { return _ledgerGuideVisible; }
            set
            {
                _ledgerGuideVisible = value;
                OnPropertyChanged(() => LedgerGuideVisible);
            }
        }

        public int Cursor_X
        {
            get { return _cursorX; }
            set
            {
                _cursorX = value;
                OnPropertyChanged(() => Cursor_X);
            }
        }

        public int Cursor_Y
        {
            get { return _cursorY; }
            set
            {
                _cursorY = value;
                OnPropertyChanged(() => Cursor_Y);
            }
        }

        public Visibility CursorVisible
        {
            get { return _cursorVisible; }
            set
            {
                _cursorVisible = value;
                OnPropertyChanged(() => CursorVisible);
            }
        }

        public string Coordinates
        {
            get { return _coordinates; }
            set
            {
                _coordinates = value;
                OnPropertyChanged(() => Coordinates);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveCommand
        {
            get { return _mouseMoveCommand; }
            set
            {
                _mouseMoveCommand = value;
                OnPropertyChanged(() => MouseMoveCommand);
            }
        }

        public void ShowLedger()
        {
            LedgerGuideVisible = Visibility.Visible;
        }

        public void HideLedgerGuide()
        {
            LedgerGuideVisible = Visibility.Collapsed;
        }

        public void ShowCursor()
        {
            if (!string.IsNullOrEmpty(EditorState.DurationType))
            {
                CursorVisible = Visibility.Visible;
            }
        }

        public void HideCursor()
        {
            CursorVisible = Visibility.Collapsed;
        }

        private void HideMarker()
        {
            ChordSelectorVisibility = Visibility.Collapsed;
        }

        private void ShowMarker()
        {
            ChordSelectorVisibility = Visibility.Visible;
            InsertMarkerVisiblity = Visibility.Collapsed;
        }

        private void HideInsertMarker()
        {
            InsertMarkerVisiblity = Visibility.Collapsed;
        }

        private void ShowInsertMarker()
        {
            InsertMarkerVisiblity = ChordSelectorVisibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            if (EditorState.IsNewCompositionPanel) return;
            Coordinates = string.Format("{0}, {1}", _measureClickX, _measureClickY);
            SwitchContext();
            if (commandParameter.EventArgs.GetType() != typeof(MouseEventArgs)) return;
            var e = commandParameter.EventArgs as MouseEventArgs;
            if (commandParameter.Parameter == null) return;
            var view = commandParameter.Parameter as UIElement;
            if (e != null)
            {
                MeasureClick_X = (int)e.GetPosition(view).X;
                MeasureClick_Y = (int)e.GetPosition(view).Y;

                if (EditorState.IsNote())
                {
                    _compositionView = null;
                    TrackLedger();
                    if (!EditorState.IsResizingMeasure)
                    {
                        TrackChordMarker();
                        TrackInsertMarker();
                    }
                }
                else
                {
                    TrackAreaSelectRectangle(e);
                }
            }
            TrackMeasureCursor();
        }

        private void TrackAreaSelectRectangle(MouseEventArgs e)
        {
            if (_compositionView == null)
            {
                _compositionView = (CompositionView)ServiceLocator.Current.GetInstance<ICompositionView>();
            }
            var pt = e.GetPosition(_compositionView);
            CompositionClickX = pt.X;
            CompositionClickY = pt.Y;
            if (EditorState.ClickState != _Enum.ClickState.First) return;
            EditorState.ClickMode = "Move";
            EA.GetEvent<AreaSelect>().Publish(new Point(CompositionClickX, CompositionClickY));
        }

        private void TrackInsertMarker()
        {
            HideInsertMarker();
            if (EditorState.IsSaving) return;
            for (var i = 0; i < ActiveChords.Count - 1; i++)
            {
                var ch1 = ActiveChords[i];
                var ch2 = ActiveChords[i + 1];
                if (MeasureClick_X <= ch1.Location_X + 24 || MeasureClick_X >= ch2.Location_X + 19) continue;
                HideLedgerGuide();

                var ns1 = ChordManager.GetActiveNotes(ch1.Notes);
                var ns2 = ChordManager.GetActiveNotes(ch2.Notes);
                var topY = ns1[0].Location_Y;
                var bottomY = ns1[0].Location_Y;
                foreach (var n in ns1)
                {
                    if (n.Location_Y < topY) topY = n.Location_Y;
                    if (n.Location_Y > bottomY) bottomY = n.Location_Y;
                }
                foreach (var n in ns2)
                {
                    if (n.Location_Y < topY) topY = n.Location_Y;
                    if (n.Location_Y > bottomY) bottomY = n.Location_Y;
                }
                TopInsertMarkerLabelMargin = topY < 5
                    ? string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY + 98, 0, 0)
                    : string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY - 4, 0, 0);

                InsertMarkerLabelPath = EditorState.IsRest() ? _insertRestPath : _insertNotePath;
                InsertMarkerColor = "Blue";

                BottomInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X + 5, bottomY + 69, 0, 0);
                TopInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X - 3, topY + 14, 0, 0);
                ShowInsertMarker();
            }
        }

        private void TrackChordMarker()
        {
            EditorState.Chord = null;
            HideMarker();
            EditorState.ReplacementMode = _Enum.ReplaceMode.None;
            if (EditorState.IsSaving) return;
            foreach (var ch in ActiveChords)
            {
                if (MeasureClick_X > ch.Location_X + 14 && MeasureClick_X < ch.Location_X + 22)
                {
                    HideLedgerGuide();
                    EditorState.Chord = ch;
                    var topY = ch.Notes[0].Location_Y;
                    var bottomY = ch.Notes[0].Location_Y;
                    var ns = ChordManager.GetActiveNotes(ch.Notes);
                    foreach (var n in ns)
                    {
                        if (n.Location_Y < topY) topY = n.Location_Y;
                        if (n.Location_Y > bottomY) bottomY = n.Location_Y;
                    }
                    if (ns[0].Type % 2 == 0 && EditorState.IsRest())
                    {
                        MarkerLabelPath = _replaceNoteWithRestPath;
                        MarkerColor = "Red";
                        EditorState.ReplacementMode = _Enum.ReplaceMode.Rest;
                    }
                    else if (ns[0].Type % 3 == 0 && !EditorState.IsRest())
                    {
                        MarkerLabelPath = _replaceRestWithNotePath;
                        MarkerColor = "Red";
                        EditorState.ReplacementMode = _Enum.ReplaceMode.Note;
                    }
                    else
                    {
                        MarkerLabelPath = _addNoteToChordPath;
                        MarkerColor = "Green";
                    }
                    TopMarkerLabelMargin = topY < 5
                        ? string.Format("{0},{1},{2},{3}", ch.Location_X + 25, topY + 106, 0, 0)
                        : string.Format("{0},{1},{2},{3}", ch.Location_X + 25, topY - 4, 0, 0);
                    BottomMarkerMargin = string.Format("{0},{1},{2},{3}", ch.Location_X + 18, bottomY + 65, 0, 0);
                    TopMarkerMargin = string.Format("{0},{1},{2},{3}", ch.Location_X + 10, topY + 14, 0, 0);
                    ShowMarker();
                }
            }
        }

        private void TrackMeasureCursor()
        {
            HideCursor();
            if (Pitch.YCoordinatePitchNormalizationMap.ContainsKey(MeasureClick_Y))
            {
                Cursor_Y = Pitch.YCoordinatePitchNormalizationMap[MeasureClick_Y];
            }
            Cursor_Y = Cursor_Y - 5;
            Cursor_X = MeasureClick_X - 13;
        }

        private void TrackLedger()
        {
            HideLedgerGuide();
            if (MeasureClick_Y <= 46)
            {
                LedgerGuide_Y = 5;
                LedgerGuide_X = MeasureClick_X - 20;
                ShowLedger();
            }
            else
            {
                if (MeasureClick_Y >= 80)
                {
                    LedgerGuide_Y = 85;
                    LedgerGuide_X = MeasureClick_X - 20;
                    ShowLedger();
                }
                else
                {
                    HideLedgerGuide();
                }
            }
        }

        public void OnHideMeasureEditHelpers(object obj)
        {
            HideVisualElements();
            HideCursor();
            HideLedgerGuide();
            HideMarker();
            HideInsertMarker();
        }

        #endregion Visual Helpers

        public Chord AddNoteToChord()
        {
            SetRepository();
            ChordManager.ActiveChords = ActiveChords;
            Chord = ChordManager.GetOrCreate(Measure.Id);
            if (Chord != null)
            {
                var n = NoteController.Create(Chord, Measure, MeasureClick_Y);
                if (n == null) return null;
                Chord.Notes.Add(n);
                Cache.Notes.Add(n);
                SetNotegroupContext();
                ChordNotegroups = NotegroupManager.ParseChord();
                SetNotegroupContext();
                var ng = NotegroupManager.GetNotegroup(n);
                if (ng != null)
                {
                    n.Orientation = ng.Orientation;
                    EA.GetEvent<FlagNotegroup>().Publish(ng);

                    var ns = GetActiveNotes(Chord.Notes);
                    if (ns.Count == 1)
                    {
                        if (Chord.Notes.Count == 1)
                        {
                            Measure.Chords.Add(Chord);
                            Cache.Chords.Add(Chord);
                            Statistics.Update(Chord.Measure_Id);
                        }
                        EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
                        _Enum.NotePlacementMode placementMode = GetPlacementMode(out _chord1, out _chord2);
                        Chord.Location_X = GetChordXCoordinate(placementMode, Chord);
                        Measure.Duration = (decimal)Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
                        _repository.Update(Measure);
                    }
                    n.Location_X = Chord.Location_X;
                }
            }
            if (EditorState.IsCollaboration)
            {
                // if this composition has collaborators, then locations and start times may need to be adjusted.
                // EA.GetEvent<MeasureLoaded>().Publish(Measure.Id);
            }
            if (Chord != null && Chord.Duration < 1)
            {
                SpanManager.LocalSpans = LocalSpans;
                EA.GetEvent<SpanMeasure>().Publish(Measure);
            }
            EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
            return Chord;
        }

        private int GetChordXCoordinate(_Enum.NotePlacementMode mode, Chord ch)
        {
            var locX = 0;
            var spacing = DurationManager.GetProportionalSpace();
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);

            switch (mode)
            {
                case _Enum.NotePlacementMode.Insert:
                    if (_chord1 != null && _chord2 != null)
                    {
                        locX = _chord1.Location_X + spacing;
                        ch.Location_X = locX;
                        ch.StartTime = _chord2.StartTime;
                        foreach (var ach in ActiveChords)  //no need to filter m.chs using GetActiveChords(). 
                        {
                            if (ach.Location_X > _chord1.Location_X && ch != ach)
                            {
                                ach.Location_X += spacing;
                                if (ach.StartTime != null) ach.StartTime = (double)ach.StartTime + (double)ch.Duration;
                                _repository.Update(ach);
                            }
                            EA.GetEvent<SynchronizeChord>().Publish(ach);
                            EA.GetEvent<UpdateChord>().Publish(ach);
                        }
                    }
                    break;
                case _Enum.NotePlacementMode.Append:
                    var a = (from c in ActiveChords where c.StartTime < Chord.StartTime select c.Location_X);
                    var e = a as List<int> ?? a.ToList();
                    locX = (!e.Any()) ? Infrastructure.Constants.Measure.Padding : Convert.ToInt32(e.Max()) + spacing;
                    break;
            }
            return locX;
        }

        public ObservableCollection<Note> GetActiveNotes(DataServiceCollection<Note> ns)
        {
            return new ObservableCollection<Note>((
                from n in ns
                where CollaborationManager.IsActive(n)
                select n).OrderBy(p => p.StartTime));
        }

        public _Enum.NotePlacementMode GetPlacementMode(out Chord ch1, out Chord ch2)
        {
            ch1 = null;
            ch2 = null;
            var clickX = MeasureClick_X + Finetune.Measure.ClickNormalizerX;
            var mode = GetChordNeighbors(out ch1, out ch2, clickX);
            return mode;
        }

        public _Enum.NotePlacementMode GetChordNeighbors(out Chord ch1, out Chord ch2, int clickX)
        {
            ch1 = null;
            ch2 = null;
            var locX1 = Defaults.MinusInfinity;
            var locX2 = Defaults.PlusInfinity;
            var mode = _Enum.NotePlacementMode.Append;

            if (!ActiveChords.Any()) return mode;
            ch1 = ActiveChords[0];
            MeasureChordNotegroups = NotegroupManager.ParseMeasure(out ChordStartTimes, out ChordInactiveTimes);
            for (var i = 0; i < ActiveChords.Count - 1; i++)
            {
                var ach1 = ActiveChords[i];
                var ach2 = ActiveChords[i + 1];

                if (clickX > ach1.Location_X && clickX < ach2.Location_X)
                {
                    ch1 = ach1;
                    ch2 = ach2;
                    mode = _Enum.NotePlacementMode.Insert;
                }
                if (clickX > ach1.Location_X && ach1.Location_X > locX1)
                {
                    ch1 = ach1;
                    locX1 = ach1.Location_X;
                }
                if (clickX < ach2.Location_X && ach2.Location_X < locX2)
                {
                    ch2 = ach2;
                    locX2 = ach2.Location_X;
                }
            }
            return mode;
        }
        
        public void OnArrangeMeasure(Repository.DataService.Measure m)
        {
            // this method calculates measure spacing then raises the Measure_Loaded event. the m.Spacing property is
            // only used to calculate chord spacing when spacingMode is 'constant.' For now, however, we call this method
            // no matter what the spaingMode is because this method raises the arrangeVerse event and the arrangeVerse event
            // should be raised for all spacingModes. TODO: decouple m spacing from verse spacing. or at the very least 
            // encapsulate the switch block in 'if then else' block so it only executes when the spacingMode is 'constant'.

            // 'EditorState.Ratio * .9' expression needs to be revisited.
            if (Measure.Id != m.Id) return;
            _measure = m;
            var chords = ChordManager.GetActiveChords(_measure.Chords);

            if (chords.Count <= 0) return;
            ChordManager.Initialize();
            SetNotegroupContext();
            _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);

            switch (Preferences.MeasureArrangeMode)
            {
                case _Enum.MeasureArrangeMode.DecreaseMeasureWidth:
                    EA.GetEvent<AdjustMeasureWidth>().Publish(new Tuple<Guid, double>(_measure.Id, Preferences.MeasureMaximumEditingSpace));
                    break;
                case _Enum.MeasureArrangeMode.IncreaseMeasureSpacing:
                    m.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(_measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / chords.Count));
                    EA.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                    break;
                case _Enum.MeasureArrangeMode.ManualResizePacked:
                    m.Spacing = Convert.ToInt32(Math.Ceiling((int.Parse(_measure.Width) - (Infrastructure.Constants.Measure.Padding * 2)) / chords.Count));
                    EA.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                    break;
                case _Enum.MeasureArrangeMode.ManualResizeNotPacked:
                    m.Spacing = (int)Math.Ceiling(m.Spacing * EditorState.Ratio * .9);
                    EA.GetEvent<MeasureLoaded>().Publish(_measure.Id);
                    break;
            }
            if (!EditorState.IsOpening)
            {
                EA.GetEvent<ArrangeVerse>().Publish(_measure);
            }
        }
    }
}