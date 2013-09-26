using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Views;
using Composer.Repository;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Dimensions;
using Measure = Composer.Repository.DataService.Measure;
using Composer.Infrastructure.Support;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class MeasureViewModel : BaseViewModel, IMeasureViewModel
    {
        DataServiceRepository<Repository.DataService.Composition> _repository;

        string addNoteToChordPath = string.Empty;
        string insertNotePath = string.Empty;
        string insertRestPath = string.Empty;
        string replaceNoteWithRestPath = string.Empty;
        string replaceRestWithNotePath = string.Empty;

        private double _widthChangeRatio;
        private double _initializedWidth;
        private int _loadedChordsCount;
        private Chord _chord;
        private Dictionary<decimal, List<Notegroup>> _measureChordNotegroups;
        public decimal StartTime;
        private double _baseRatio;
        private decimal[] _chordStartTimes;
        private decimal[] _chordInactiveTimes;
        private bool _isMouseCaptured;
        private bool okToResize = true;
        private double _mouseY;
        private double _mouseX;
        private double _measureBarBeforeDragX;
        private double _measureBarAfterDragX;
        private int _inactiveChordCnt;
        private bool _debugging = false;

        public MeasureView View;

        private void SetChordContext()
        {
            ChordManager.Location_X = MeasureClick_X;
            ChordManager.Location_Y = MeasureClick_Y;
            ChordManager.ChordNotegroups = null;
            ChordManager.Measure = Measure;
        }

        private void SetNotegroupContext()
        {
            NotegroupManager.ChordStarttimes = _chordStartTimes;
            NotegroupManager.ChordNotegroups = null;
            NotegroupManager.Measure = Measure;
            NotegroupManager.Chord = _chord;
        }

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
            setActiveChords(true);
            setDuration();
            setActiveMeasureCount();
            _widthChangeRatio = GetRatio();
            _baseRatio = _widthChangeRatio;
            EA.GetEvent<SetMeasureEndBar>().Publish(string.Empty);
            this.PlaybackControlVisibility = Visibility.Collapsed;
            SetTextPaths();
        }

        private void setActiveChords(bool isloading)
        {
            //this is the first time IsActionable is called for ns in a loading composition....
            EA.GetEvent<UpdateActiveChords>().Publish(Measure.Id);
            //...so, at this point in the flow, every n in the m has been activated or deactivated.
        }

        private void setActiveMeasureCount()
        {
            //why are we excluding the first m - (a.Index > 0 )?
            EditorState.ActiveMeasureCount = (from a in Cache.Measures where ActiveChords.Count > 0 && a.Index > 0 select a).DefaultIfEmpty(null).Count();
        }

        private void setDuration()
        {
            Duration = (decimal)Convert.ToDouble((from c in ActiveChords select c.Duration).Sum());
        }

        private void SetTextPaths()
        {
            if (EditorState.UseVerboseMouseTrackers)
            {
                addNoteToChordPath = (from a in Vectors.VectorList where a.Name == "AddNoteToChord" select a.Path).First().ToString();
                insertNotePath = (from a in Vectors.VectorList where a.Name == "InsertNote" select a.Path).First().ToString();
                insertRestPath = (from a in Vectors.VectorList where a.Name == "InsertRest" select a.Path).First().ToString();
                replaceNoteWithRestPath = (from a in Vectors.VectorList where a.Name == "ReplaceNoteWithRest" select a.Path).First().ToString();
                replaceRestWithNotePath = (from a in Vectors.VectorList where a.Name == "ReplaceRestWithNote" select a.Path).First().ToString();
            }
            else
            {
                addNoteToChordPath = (from a in Vectors.VectorList where a.Name == "Add" select a.Path).First().ToString();
                insertNotePath = (from a in Vectors.VectorList where a.Name == "Insert" select a.Path).First().ToString();
                insertRestPath = (from a in Vectors.VectorList where a.Name == "Insert" select a.Path).First().ToString();
                replaceNoteWithRestPath = (from a in Vectors.VectorList where a.Name == "Replace" select a.Path).First().ToString();
                replaceRestWithNotePath = (from a in Vectors.VectorList where a.Name == "Replace" select a.Path).First().ToString();
            }
        }

        private Visibility _playbackControlVisibility = Visibility.Collapsed;
        public Visibility PlaybackControlVisibility
        {
            get { return _playbackControlVisibility; }
            set
            {
                _playbackControlVisibility = value;
                OnPropertyChanged(() => PlaybackControlVisibility);
            }
        }

        private ObservableCollection<Chord> _activeChords;
        public ObservableCollection<Chord> ActiveChords
        {
            get { return _activeChords; }
            set
            {
                _activeChords = value;
                _activeChords = new ObservableCollection<Chord>(_activeChords.OrderBy(p => p.StartTime));
                OnPropertyChanged(() => ActiveChords);
            }
        }

        private double GetRatio()
        {
            double ratio = 1;
            if (EditorState.IsOpening)
            {
                if (ActiveChords.Count > 1)
                {
                    var actualProportionalSpacing = ActiveChords[1].Location_X - ActiveChords[0].Location_X;
                    double defaultProportionalSpacing = DurationManager.GetProportionalSpace((double)ActiveChords[0].Duration);
                    ratio = actualProportionalSpacing / defaultProportionalSpacing;
                }
            }
            else
            {
                ratio = Width / _initializedWidth;
            }
            return ratio;
        }

        private decimal _duration;
        public decimal Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged(() => Duration);
            }
        }

        private string _barBackground = Preferences.BarBackground;
        public string BarBackground
        {
            get { return _barBackground; }
            set
            {
                _barBackground = value;
                OnPropertyChanged(() => BarBackground);
            }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                OnPropertyChanged(() => ImageUrl);
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                OnPropertyChanged(() => FirstName);
            }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged(() => LastName);
            }
        }

        private string _background = Preferences.MeasureBackground;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string _foreground = Preferences.MeasureForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string _barForeground = Preferences.BarForeground;
        public string BarForeground
        {
            get { return _barForeground; }
            set
            {
                _barForeground = value;
                OnPropertyChanged(() => BarForeground);
            }
        }

        private string _barMargin = "0";

        public string BarMargin
        {
            get { return _barMargin; }
            set
            {
                _barMargin = value;
                OnPropertyChanged(() => BarMargin);
            }
        }

        private ObservableCollection<Verse> _subVerses;
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
                if (value != _timeSignatureId)
                {
                    _timeSignatureId = value;
                    OnPropertyChanged(() => TimeSignature_Id);

                    var timeSignature = (from a in TimeSignatures.TimeSignatureList
                                         where a.Id == _timeSignatureId
                                         select a.Name).First();

                    if (string.IsNullOrEmpty(timeSignature))
                    {
                        timeSignature = (from a in TimeSignatures.TimeSignatureList where a.Id == Preferences.DefaultTimeSignatureId select a.Name).First();
                    }

                    DurationManager.BPM = Int32.Parse(timeSignature.Split(',')[0]);
                    DurationManager.BeatUnit = Int32.Parse(timeSignature.Split(',')[1]);
                    DurationManager.Initialize();
                    StartTime = (Measure.Index) * DurationManager.BPM;
                }
            }
        }

        private Measure _measure;
        public Measure Measure
        {
            get { return _measure; }
            set
            {
                _measure = value;
                Background = Preferences.MeasureBackground;
                Bar_Id = Measure.Bar_Id;
                EA.GetEvent<ShowMeasureFooter>().Publish(_Enum.MeasureFooter.Editing);
                Duration = _measure.Duration;
                OnPropertyChanged(() => Measure);
            }
        }

        private short _barId;
        public short Bar_Id
        {
            get { return _barId; }
            set
            {
                _barId = value;
                Measure.Bar_Id = _barId;
                BarMargin = (from a in Bars.BarList where a.Id == _barId select a.Margin).First();
                OnPropertyChanged(() => Bar_Id);
            }
        }

        private ObservableCollection<LocalSpan> _localSpans;
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
            if (Infrastructure.Support.Selection.Notes.Any() || Infrastructure.Support.Selection.Arcs.Any())
            {
                //there's an active selection, so stop here and use this click to deselect all selected ns
                EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                return;
            }
            //notify the parent staff about the click so the staff can do 
            //whatever it needs to do when a _measure is clicked.
            EA.GetEvent<SendMeasureClickToStaff>().Publish(Measure.Staff_Id);
            //remove active _measure status from all Measures
            EA.GetEvent<SetMeasureBackground>().Publish(Guid.Empty);
            //make this m the active _measure
            EA.GetEvent<SetMeasureBackground>().Publish(Measure.Id);

            if (EditorState.DurationSelected())
            {
                //...the user has clicked on the m with a n or n tool.
                EditorState.Duration = (from a in DurationManager.Durations
                                        where (a.Caption == EditorState.DurationCaption)
                                        select a.Value).DefaultIfEmpty(Constants.INVALID_DURATION).Single();
                if (ValidPlacement())
                {
                    SetChordContext();
                    _chord = ChordManager.AddNoteToChord(this);
                    //TODO: Why am I updating the provenance panel every time I click a m?
                    EA.GetEvent<UpdateProvenancePanel>().Publish(CompositionManager.Composition);
                }
            }
            else
            {
                //the user clicked with a tool that is not a n or n. route click to tool dispatcher
                OnToolClick();
            }
            setActiveChords(false);
            setDuration();
            adjustEndSpace();
        }

        public void adjustEndSpace()
        {
            if (MeasureManager.IsPackedMeasure(Measure))
            {
                AdjustTrailingSpace(Measure.Id, Preferences.MeasureMaximumEditingSpace);
            }
            else
            {
                if (ActiveChords.Count > 0)
                {
                    Chord lastChord = (from c in ActiveChords select c).OrderBy(p => p.StartTime).Last();
                    if (lastChord.Location_X + Preferences.MeasureMaximumEditingSpace > Width)
                    {
                        AdjustTrailingSpace(Measure.Id, Preferences.MeasureMaximumEditingSpace);
                    }
                }
            }
        }

        public bool ValidPlacement()
        {
            bool validity = true;
            try
            {
                bool isPackedMeasure = MeasureManager.IsPackedMeasure(Measure);
                bool isAddingToChord = IsAddingToChord();
                if (EditorState.Duration != Constants.INVALID_DURATION)
                {
                    if (isPackedMeasure && !isAddingToChord)
                    {
                        //commented out 09222013
                        //if (_chordStartTimes == null)
                        //{
                        //    SetNotegroupContext();
                        //    _measureChordNotegroups = NotegroupManager.ParseMeasure(out _chordStartTimes, out _chordInactiveTimes);
                        //}
                        //EA.GetEvent<ArrangeMeasure>().Publish(_measure);
                        validity = false;
                    }
                    else
                    {
                        validity = !(Duration + (decimal)EditorState.Duration > DurationManager.BPM && !isAddingToChord);
                    }
                }
            }
            catch (Exception ex)
            {
                validity = false;
                Exceptions.HandleException(ex);
            }
            return validity;
        }

        public void OnToolClick()
        {
            var p = Infrastructure.Support.Utilities.CoordinateSystem.TranslateToCompositionCoords
                (
                    MeasureClick_X,
                    MeasureClick_Y,
                    Measure.Sequence,
                    Measure.Index,
                    (double)StartTime,
                    DurationManager.BPM,
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

        #region Visual Helpers

        private int _measureClickY;
        public int MeasureClick_Y
        {
            get { return _measureClickY; }
            set
            {
                if (value != _measureClickY)
                {
                    _measureClickY = value;
                    OnPropertyChanged(() => MeasureClick_Y);
                }
            }
        }

        private int _measureClickX;
        public int MeasureClick_X
        {
            get { return _measureClickX; }
            set
            {
                if (value != _measureClickX)
                {
                    _measureClickX = value;
                    OnPropertyChanged(() => MeasureClick_X);
                }
            }
        }

        public double CompositionClickX { get; set; }
        public double CompositionClickY { get; set; }

        private CompositionView _compositionView;

        private int _ledgerGuideX;

        public int LedgerGuide_X
        {
            get { return _ledgerGuideX; }
            set
            {
                _ledgerGuideX = value;
                OnPropertyChanged(() => LedgerGuide_X);
            }
        }

        private int _ledgerGuideY;

        public int LedgerGuide_Y
        {
            get { return _ledgerGuideY; }
            set
            {
                _ledgerGuideY = value;
                OnPropertyChanged(() => LedgerGuide_Y);
            }
        }

        private Visibility _ledgerGuideVisible = Visibility.Collapsed;
        public Visibility LedgerGuideVisible
        {
            get { return _ledgerGuideVisible; }
            set
            {
                _ledgerGuideVisible = value;
                OnPropertyChanged(() => LedgerGuideVisible);
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

        private int _cursorX;

        public int Cursor_X
        {
            get { return _cursorX; }
            set
            {
                _cursorX = value;
                OnPropertyChanged(() => Cursor_X);
            }
        }

        private int _cursorY;

        public int Cursor_Y
        {
            get { return _cursorY; }
            set
            {
                _cursorY = value;
                OnPropertyChanged(() => Cursor_Y);
            }
        }

        private Visibility _cursorVisible = Visibility.Collapsed;
        public Visibility CursorVisible
        {
            get { return _cursorVisible; }
            set
            {
                _cursorVisible = value;
                OnPropertyChanged(() => CursorVisible);
            }
        }

        private string _coordinates;
        public string Coordinates
        {
            get { return _coordinates; }
            set
            {
                _coordinates = value;
                OnPropertyChanged(() => Coordinates);
            }
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
            if (ChordSelectorVisibility == Visibility.Collapsed)
            {
                InsertMarkerVisiblity = Visibility.Visible;
            }
            else
            {
                InsertMarkerVisiblity = Visibility.Collapsed;
            }
        }
        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            if (!EditorState.IsNewCompositionPanel)
            {
                Coordinates = string.Format("{0}, {1}", _measureClickX, _measureClickY);

                SwitchContext();
                if (commandParameter.EventArgs.GetType() == typeof(MouseEventArgs))
                {
                    var e = commandParameter.EventArgs as MouseEventArgs;
                    if (commandParameter.Parameter != null)
                    {
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
                }
            }
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
            if (EditorState.ClickState == _Enum.ClickState.First)
            {
                EditorState.ClickMode = "Move";
                EA.GetEvent<AreaSelect>().Publish(new Point(CompositionClickX, CompositionClickY));
            }
        }

        private void TrackInsertMarker()
        {
            HideInsertMarker();
            if (!EditorState.IsSaving)
            {
                for (var i = 0; i < ActiveChords.Count - 1; i++)
                {
                    Chord ch1 = ActiveChords[i];
                    Chord ch2 = ActiveChords[i + 1];
                    if (MeasureClick_X > ch1.Location_X + 24 && MeasureClick_X < ch2.Location_X + 19)
                    {
                        HideLedgerGuide();

                        var notes1 = ChordManager.GetActiveNotes(ch1.Notes);
                        var notes2 = ChordManager.GetActiveNotes(ch2.Notes);
                        int topY = notes1[0].Location_Y;
                        int bottomY = notes1[0].Location_Y;
                        foreach (Note note in notes1)
                        {
                            if (note.Location_Y < topY) topY = note.Location_Y;
                            if (note.Location_Y > bottomY) bottomY = note.Location_Y;
                        }
                        foreach (Note note in notes2)
                        {
                            if (note.Location_Y < topY) topY = note.Location_Y;
                            if (note.Location_Y > bottomY) bottomY = note.Location_Y;
                        }
                        if (topY < 5)
                        {
                            TopInsertMarkerLabelMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY + 98, 0, 0);
                        }
                        else
                        {
                            TopInsertMarkerLabelMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY - 4, 0, 0);
                        }
                        InsertMarkerLabelPath = EditorState.IsRest() ? insertRestPath : insertNotePath;
                        InsertMarkerColor = "Blue";

                        BottomInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X + 5, bottomY + 69, 0, 0);
                        TopInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X - 3, topY + 14, 0, 0);
                        ShowInsertMarker();
                    }
                }
            }
        }

        private void TrackChordMarker()
        {
            EditorState.Chord = null;
            HideMarker();
            EditorState.ReplacementMode = _Enum.ReplaceMode.None;
            if (!EditorState.IsSaving)
            {
                foreach (Chord chord in ActiveChords)
                {
                    if (MeasureClick_X > chord.Location_X + 14 && MeasureClick_X < chord.Location_X + 22)
                    {
                        HideLedgerGuide();
                        EditorState.Chord = chord;
                        var topY = chord.Notes[0].Location_Y;
                        var bottomY = chord.Notes[0].Location_Y;
                        var notes = ChordManager.GetActiveNotes(chord.Notes);
                        foreach (Note note in notes)
                        {
                            if (note.Location_Y < topY) topY = note.Location_Y;
                            if (note.Location_Y > bottomY) bottomY = note.Location_Y;
                        }
                        if (notes[0].Type % 2 == 0 && EditorState.IsRest())
                        {
                            MarkerLabelPath = replaceNoteWithRestPath;
                            MarkerColor = "Red";
                            EditorState.ReplacementMode = _Enum.ReplaceMode.Rest;
                        }
                        else if (notes[0].Type % 3 == 0 && !EditorState.IsRest())
                        {
                            MarkerLabelPath = replaceRestWithNotePath;
                            MarkerColor = "Red";
                            EditorState.ReplacementMode = _Enum.ReplaceMode.Note;
                        }
                        else
                        {
                            MarkerLabelPath = addNoteToChordPath;
                            MarkerColor = "Green";
                        }
                        if (topY < 5)
                        {
                            TopMarkerLabelMargin = string.Format("{0},{1},{2},{3}", chord.Location_X + 25, topY + 106, 0, 0);
                        }
                        else
                        {
                            TopMarkerLabelMargin = string.Format("{0},{1},{2},{3}", chord.Location_X + 25, topY - 4, 0, 0);
                        }

                        BottomMarkerMargin = string.Format("{0},{1},{2},{3}", chord.Location_X + 18, bottomY + 65, 0, 0);
                        TopMarkerMargin = string.Format("{0},{1},{2},{3}", chord.Location_X + 10, topY + 14, 0, 0);
                        ShowMarker();
                    }
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

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveCommand;

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveCommand
        {
            get { return _mouseMoveCommand; }
            set
            {
                _mouseMoveCommand = value;
                OnPropertyChanged(() => MouseMoveCommand);
            }
        }

        public void OnHideMeasureEditHelpers(object obj)
        {
            base.HideVisualElements();
            HideCursor();
            HideLedgerGuide();
            HideMarker();
            HideInsertMarker();
        }

        #endregion Visual Helpers

        public void SubscribeEvents()
        {
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

        public void OnUpdateActiveChords(Guid id)
        {
            ObservableCollection<Chord> activeChords = ActiveChords;
            if (id == Measure.Id)
            {
                activeChords = new ObservableCollection<Chord>((
                    from a in Measure.Chords
                    where CollaborationManager.IsActive(a)
                    select a).OrderBy(p => p.StartTime));
            }
            EA.GetEvent<NotifyActiveChords>().Publish(new Tuple<Guid, object>(Measure.Id, activeChords));
        }

        public void OnNotifyActiveChords(Tuple<Guid, object> payload)
        {
            Guid mId = payload.Item1;
            if (mId == Measure.Id)
            {
                ActiveChords = (ObservableCollection<Chord>)payload.Item2;
            }
        }

        public void OnUpdateMeasureBarX(Tuple<Guid, double> payload)
        {
            var m = (from a in Cache.Measures where a.Id == payload.Item1 select a).First();
            if (m.Sequence == Measure.Sequence)
            {
                try
                {
                    string s = payload.Item2.ToString();
                    MeasureBar_X = int.Parse(s);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void OnUpdateMeasureBarColor(Tuple<Guid, string> payload)
        {
            var m = (from a in Cache.Measures where a.Id == payload.Item1 select a).First();
            if (m.Sequence == Measure.Sequence)
            {
                BarForeground = payload.Item2;
            }
        }

        public void OnSetPlaybackControlVisibility(Guid id)
        {
            if (id == Measure.Id)
            {
                var s = (from a in Cache.Staffs where a.Id == Measure.Staff_Id select a).First();
                if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                   (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && s.Index % 2 == 0))
                {
                    this.PlaybackControlVisibility = (Measure.Chords.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
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
            this.EditingFooterVisible = Visibility.Visible;
        }

        public void OnShowSavePanel(object obj)
        {
            this.EditingFooterVisible = Visibility.Collapsed;
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
                if (Measure.Sequence == (Composer.Infrastructure.Support.Densities.MeasureDensity - 1) * Defaults.SequenceIncrement)
                {
                    var parentStaff = (from a in Cache.Staffs where a.Id == Measure.Staff_Id select a).First();
                    var parentStaffgroup = (from a in Cache.Staffgroups where a.Id == parentStaff.Staffgroup_Id select a).First();
                    if (parentStaffgroup.Sequence == (Composer.Infrastructure.Support.Densities.StaffgroupDensity - 1) * Defaults.SequenceIncrement)
                    {
                        if (Measure.Bar_Id == Bars.StandardBarId)
                        {
                            Bar_Id = Bars.EndBarId;
                        }
                        else
                        {
                            //if we arrive here then the endbar was intentionally set to whatever it is. leave it alone.
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public void OnUpdateMeasureBar(short barId)
        {
            //this event is broadcast to all measures. if this m has a enbar with endbarid=Bars.EndBarId, (if it 
            //is the last m in the last staffgroup), then it is reset to the barid passed in.
            if (EditorState.IsAddingStaffgroup)
            {
                if (Bar_Id == Bars.EndBarId)
                {
                    Bar_Id = barId;
                }
            }
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            var state = (TranspositionState)payload.Item2;
            Measure.Key_Id = state.Key.Id;
        }

        public void OnAdjustMeasureWidth(Tuple<Guid, double> payload)
        {
            //when a _measure is not packed, but there's no room to add another ch, the
            //AdjustMeasureWidth event is raised.
            Guid id = payload.Item1;
            double endSpace = payload.Item2;
            if (id == Measure.Id)
            {
                if (ActiveChords.Count > 0)
                {
                    //set the _measure width to the x coordinate of the last ch in the _measure plus an integer value passed 
                    //in via the event payload - usually Preferences.MeasureMaximumEditingSpace * _measure spc ratio.

                    //get the last ch in the m, then...
                    var chord = (from c in ActiveChords select c).OrderBy(q => q.StartTime).Last();

                    //...add the calculated (passed in) width to get the new m Width
                    int maxWidthInSequence = int.Parse((from c in Cache.Measures where c.Sequence == Measure.Sequence select c.Width).Max());
                    int proposedWidth = chord.Location_X + (int)Math.Floor(endSpace);

                    //the "Width = ch..." line above sets the width of the m. "ResizeMeasure" below also sets the width 
                    //of the m, among other things. however, if you comment out the line "Width = ch...." above so that we 
                    //are setting the width only once, results are unpredictable. so we are setting the width twice pending a real solution.

                    //NOTE: we have to set the width in ResizeMeasure so that the width is broadcast to all measures in the same seq.

                    //TODO; the parameter (payload) for AdjustMeasureWidth event should be what it is for the ResizeMeasure event 
                    //so that the "if (id == _measure.Id)" test above can become "if (seq == _measure.Sequence)". that way we won't
                    //have to call ResizeMeasure since AdjustMeasureWidth will be broadcast to all measures with the same seq just 
                    //like ResizeMeasure is.

                    if (okToResize)
                    {
                        if (proposedWidth > maxWidthInSequence)
                        {
                            Width = proposedWidth;
                            EA.GetEvent<ResizeMeasure>().Publish(new MeasureWidthChangePayload { Id = Measure.Id, Sequence = Measure.Sequence, Width = proposedWidth, StaffgroupId = Guid.Empty });
                        }
                        else
                        {
                            _Enum.MeasureArrangeMode currentAction = Preferences.MeasureArrangeMode;
                            Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.IncreaseMeasureSpacing;

                            EditorState.NoteSpacingRatio = (double)maxWidthInSequence / (double)proposedWidth;
                            EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                            EditorState.NoteSpacingRatio = 1;

                            Preferences.MeasureArrangeMode = currentAction;
                        }
                    }
                }
            }
        }

        private void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }

        public void OnDeleteTrailingRests(Guid id)
        {
            SetRepository();
            var deletedNotes = new List<Guid>();
            foreach (Chord d in ActiveChords)
            {
                if (d.Notes[0].Pitch == "R")
                    deletedNotes.Add(d.Notes[0].Id);
                else
                    break;
            }

            foreach (Guid noteId in deletedNotes)
            {
                EA.GetEvent<DeleteEntireChord>().Publish(new Tuple<Guid, Guid>(Measure.Id, noteId));
            }
        }

        public void OnDeleteEntireChord(Tuple<Guid, Guid> payload)
        {
            if (payload.Item1 != Measure.Id) return;
            SetRepository();
            var note = (from a in Cache.Notes where a.Id == payload.Item2 select a).First();
            if (CollaborationManager.IsActive(note))
            {
                var chord = (from a in Cache.Chords where a.Id == note.Chord_Id select a).First();

                //get the n in the ch with the least d.
                decimal d = (from c in chord.Notes select c.Duration).DefaultIfEmpty<decimal>(0).Min();

                var ids = chord.Notes.Select(n => n.Id).ToList();
                foreach (var id in ids)
                {
                    note = (from a in Cache.Notes where a.Id == id select a).First();
                    _repository.Delete(note);
                    Cache.Notes.Remove(note);
                    chord.Notes.Remove(note);
                    note = NoteController.Deactivate(note);
                }

                Measure.Chords.Remove(chord);
                _repository.Delete(chord);
                Cache.Chords.Remove(chord);
                Measure.Duration -= d;
                Measure.Duration = Math.Max(0, Measure.Duration); //we have not seen a negative calculated here, but just in case...
                foreach (Chord ch in ActiveChords)
                {
                    if (ch.Location_X > note.Location_X)
                    {
                        ch.StartTime = ch.StartTime - (double)note.Duration;
                        EA.GetEvent<SynchronizeChord>().Publish(ch);
                        EA.GetEvent<UpdateChord>().Publish(ch);
                    }
                }
                setActiveChords(false);
                setDuration();
            }
        }

        public void OnBroadcastNewMeasureRequest(object obj)
        {
            if (obj == null) return;
            var measure = (Measure)obj;
            if (Measure.Index != measure.Index) return;
            //the next paste target _measure (calculeted in GetNextPasteTarget()) is sent to the EditPopupMenu ViewModel.
            EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
            //send 'Paste' command as if it was selected on the Edit Popup Menu (or control-V);
            EA.GetEvent<EditPopupItemClicked>().Publish(new Tuple<string, string, _Enum.PasteCommandSource>(Infrastructure.Constants.EditActions.Paste, "", _Enum.PasteCommandSource.Programmatic));
        }

        public void OnClearVerses(object obj)
        {
            SubVerses = new ObservableCollection<Verse>();
        }

        public void OnDeSelectMeasure(Guid id)
        {
            if (Measure.Id == id)
            {
                FooterSelectAllVisibility = Visibility.Collapsed;
                FooterSelectAllText = "Select";
                foreach (var ch in ActiveChords)
                {
                    EA.GetEvent<DeSelectChord>().Publish(ch.Id);
                }
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
            //this method determines when the _measure is loaded by tracking the number of loaded chs.
            //when the number of loaded chs is = to the number of chs in the m then we publish 
            //MeassureLoaded event, and then unsubscribe. only needed when a composition is loaded.
            if (id == Measure.Id)
            {
                _loadedChordsCount++;
                if (_loadedChordsCount == Measure.Chords.Count())
                {
                    if (Measure.Chords.Any())
                    {
                        EA.GetEvent<MeasureLoaded>().Publish(Measure.Id);
                    }
                    EA.GetEvent<NotifyChord>().Unsubscribe(OnNotifyChord);
                }
            }
        }

        private void ProcessLyrics()
        {
            CompositionManager.Composition.Verses.OrderBy(p => p.Index);
            Cache.Verses = CompositionManager.Composition.Verses;
            EA.GetEvent<UpdateVerseIndexes>().Publish(Cache.Verses.Count);
        }

        private bool CheckAllActiveMeasuresLoaded()
        {
            EditorState.LoadedActiveMeasureCount++;
            return EditorState.ActiveMeasureCount == EditorState.LoadedActiveMeasureCount;
        }

        public void OnMeasureLoaded(Guid id)
        {
            //some chs in a _measure may not be actionable (inactive), so they aren't visible, and void of meaning. 
            //the side effect  of this is that some information needed to accurately place a ch spatially may not
            //be known until after all chs in the _measure have been loaded. so after the _measure is loaded, this
            //event fires, and all chs are touched again, and adjusted if necessary.

            //NOTE: this handler is called in many more situations than originally intended (see comments above for original intent.)
            //it's a fairly substantial efforrt to refactor

            //verse numbers appear in the first _measure (idx = 1) _measure only. right now, the verse 
            //margin is the same whether ns exist or not. but leave ability to vary margin anyway.
            VerseMargin = (Measure.Index == 1) ? "8,-5,0,0" : "8,-5,0,0";

            //we need to know when a saved composition has finished loading. a surrogate for this can be when the number
            //of loaded measures equals the number of meaasures in the composition. So, track the number of loadedmeasure.
            EditorState.RunningLoadedMeasureCount++;

            //is this the view model for the target _measure?
            if (Measure.Id == id)
            {
                if (ActiveChords.Any())
                {
                    EA.GetEvent<SetPlaybackControlVisibility>().Publish(Measure.Id);
                    //lyrics are aligned to the x coordinate of respective chs, so we can't load lyrics until all chs have rendered, and.... 
                    //.....all chs have rendered when all m have loaded.
                    if (CheckAllActiveMeasuresLoaded())
                    {
                        EditorState.IsContextSwitch = false;
                        ProcessLyrics();
                        EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
                        if (!EditorState.ArcsLoaded)
                        {
                            EA.GetEvent<BroadcastArcs>().Publish(CompositionManager.Composition.Arcs);
                            EditorState.ArcsLoaded = true;
                        }
                        EA.GetEvent<ArrangeVerse>().Publish(Measure);
                        EA.GetEvent<ArrangeArcs>().Publish(Measure);
                        EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);
                        EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
                    }

                    decimal[] chordStarttimes;
                    decimal[] chordInactiveTimes;
                    decimal[] chordActiveTimes;
                    var activeChordIndex = 0;
                    var prevChordId = Guid.Empty;
                    SetNotegroupContext();
                    _measureChordNotegroups = NotegroupManager.ParseMeasure(out chordStarttimes, out chordInactiveTimes, out chordActiveTimes, ActiveChords);
                    foreach (var starttime in chordActiveTimes) //on 10/1/2012 changed chordStarttimes to chordActiveTimes
                    {
                        foreach (var chord in ActiveChords)
                        {
                            if (chord.StartTime != (double)starttime) continue;
                            chord.Duration = ChordManager.SetDuration(chord);
                            if (_widthChangeRatio == 0) _widthChangeRatio = GetRatio();

                            var payload = new Tuple<int, int, Guid, Guid, int, double>(
                                activeChordIndex,
                                _inactiveChordCnt,
                                chord.Id,
                                prevChordId,
                                _measure.Spacing,
                                _widthChangeRatio);

                            EA.GetEvent<SetChordLocationX>().Publish(payload);
                            if (chordInactiveTimes.Contains(starttime))
                            {
                                _inactiveChordCnt++;
                            }
                            else
                            {
                                //when spc for a ch is deteremined, that value is added to the x coordinate of the previous ch
                                //to get the current ch location_x. that's why we track the previous ch.
                                prevChordId = chord.Id;
                            }
                            break;
                        }
                        activeChordIndex++;
                    }
                    if (MeasureManager.IsPackedMeasure(Measure))
                    {
                        //...then make sure end bar is proportionally spaced after last ch
                        okToResize = false;
                        AdjustTrailingSpace(Measure.Id, Preferences.MeasureMaximumEditingSpace);
                        okToResize = true;
                    }
                    SpanManager.LocalSpans = LocalSpans;
                    EA.GetEvent<SpanMeasure>().Publish(Measure);
                    _inactiveChordCnt = 0;
                }
            }

            if (EditorState.RunningLoadedMeasureCount == Infrastructure.Support.Densities.MeasureCount)
            {
                var staff = (from a in Cache.Staffs
                             where a.Id == _measure.Staff_Id
                             select a).DefaultIfEmpty(null).Single();

                double staffWidth = (from a in staff.Measures select double.Parse(a.Width)).Sum() +
                                    Defaults.StaffDimensionWidth +
                                    Defaults.CompositionLeftMargin - 70;

                EditorState.GlobalStaffWidth = staffWidth;
                EA.GetEvent<SetSocialChannels>().Publish(string.Empty);
                EA.GetEvent<SetRequestPrompt>().Publish(string.Empty);
                EditorState.IsOpening = false; //composition has finished opening and is ready to edit.
            }
        }

        private void AdjustTrailingSpace(Guid measureId, double defaultEndSpace)
        {
            //we want the space between the last ch and the m endbar to be proportional to the n spc.
            //the 'w' passed in is the end spc that a m of default width would have. if the m has been
            //resized, then 'w' needs to be adjusted proportionaly. 

            double proportionallyAdjustedEndSpace = defaultEndSpace * _widthChangeRatio * _baseRatio;

            //however, for aesthetic reasons, there is a minimum endspace below which we do not want to go 
            //below, and maximum endspace we don't want to go above.

            if (proportionallyAdjustedEndSpace > Preferences.MeasureMaximumEditingSpace)
                proportionallyAdjustedEndSpace = Preferences.MeasureMaximumEditingSpace;
            else
                if (proportionallyAdjustedEndSpace < Preferences.MeasureMinimumEditingSpace)
                    proportionallyAdjustedEndSpace = Preferences.MeasureMinimumEditingSpace;

            //the handler for the AdjustMeasureWidth event will find the x coordinate of the last ch in the m, then 
            //add 'w' to it for the new m width.

            EA.GetEvent<AdjustMeasureWidth>().Publish(new Tuple<Guid, double>(Measure.Id, proportionallyAdjustedEndSpace));
        }

        public void OnApplyVerse(Tuple<object, int, int, Guid, int, int> payload)
        {
            //Repository.DataService.Verse collection is a member of Repository.DataService.Composition, but we don't 
            //bind to this collection because the binding scope needs to be Repository.DataService._measure. ie: the 
            //storage scope of verses is composition level, but the binding Scope is the _measure. I don't want to 
            //spin up temporary subcollections of Repository.DataService.Verse objects. Instead, spin up a different 
            //collection of Verses to bind too. This design choice also helps facilitate the projection of verse text 
            //into words (we don't persist words as a seperate entity, but each word still has a view, viewmodel, etc)

            Guid id = payload.Item4;

            if (id == Measure.Id)  //is this the measureViewModel for the tareget _measure?
            {
                var words = (ObservableCollection<Word>)payload.Item1;
                int index = payload.Item3;
                var v = new Verse(index, id.ToString()) { Words = words, VerseText = string.Empty, Disposition = payload.Item5 };

                if (SubVerses == null)
                    SubVerses = new ObservableCollection<Verse>();

                SubVerses.Add(v);

                var sv = new List<Verse>();
                sv.AddRange(SubVerses.OrderBy(i => i.Index));
                SubVerses = new ObservableCollection<Verse>(sv);

                //verseCount required for trestleHeight calculation;
                EditorState.VerseCount = CompositionManager.Composition.Verses.Count;
                //force bind to new trestleHeight value by setting EmptyBind to anything.
                EmptyBind = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void DefineCommands()
        {
            ClickCommand = new DelegatedCommand<object>(OnClick);
            MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, null);

            MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, null);

            MouseLeaveBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeaveBar, null);
            MouseEnterBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterBar, null);
            MouseLeftButtonUpBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonUpOnBar, null);
            MouseLeftButtonDownBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnBar, null);
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
        public Visibility BarVisibility
        {
            get { return _barVisibility; }
            set
            {
                _barVisibility = value;
                OnPropertyChanged(() => BarVisibility);
            }
        }

        private DelegatedCommand<object> _clickFooterAcceptAllCommand;

        public DelegatedCommand<object> ClickFooterAcceptAllCommand
        {
            get { return _clickFooterAcceptAllCommand; }
            set
            {
                _clickFooterAcceptAllCommand = value;
                OnPropertyChanged(() => ClickFooterAcceptAllCommand);
            }
        }

        private DelegatedCommand<object> _clickFooterRejectAllCommand;

        public DelegatedCommand<object> ClickFooterRejectAllCommand
        {
            get { return _clickFooterRejectAllCommand; }
            set
            {
                _clickFooterRejectAllCommand = value;
                OnPropertyChanged(() => ClickFooterRejectAllCommand);
            }
        }

        private DelegatedCommand<object> _clickFooterCompareCommand;

        public DelegatedCommand<object> ClickFooterCompareCommand
        {
            get { return _clickFooterCompareCommand; }
            set
            {
                _clickFooterCompareCommand = value;
                OnPropertyChanged(() => ClickFooterCompareCommand);
            }
        }

        private DelegatedCommand<object> _clickFooterPickCommand;
        public DelegatedCommand<object> ClickFooterPickCommand
        {
            get { return _clickFooterPickCommand; }
            set
            {
                _clickFooterPickCommand = value;
                OnPropertyChanged(() => ClickFooterPickCommand);
            }
        }

        private DelegatedCommand<object> _clickFooterSelectAllCommand;
        public DelegatedCommand<object> ClickFooterSelectAllCommand
        {
            get { return _clickFooterSelectAllCommand; }
            set
            {
                _clickFooterSelectAllCommand = value;
                OnPropertyChanged(() => ClickFooterSelectAllCommand);
            }
        }

        private DelegatedCommand<object> _clickFooterDeleteCommand;
        public DelegatedCommand<object> ClickFooterDeleteCommand
        {
            get { return _clickFooterDeleteCommand; }
            set
            {
                _clickFooterDeleteCommand = value;
                OnPropertyChanged(() => ClickFooterDeleteCommand);
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
                        var s = (from a in Cache.Staffs where a.Id == Measure.Staff_Id select a).First();
                        if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple ||
                           (EditorState.StaffConfiguration == _Enum.StaffConfiguration.Grand && s.Index % 2 == 0))
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

        private string _footerSelectAllText = "Select";
        public string FooterSelectAllText
        {
            get { return _footerSelectAllText; }
            set
            {
                _footerSelectAllText = value;
                OnPropertyChanged(() => FooterSelectAllText);
            }
        }

        private Visibility _footerSelectAllVisibility = Visibility.Collapsed;
        public Visibility FooterSelectAllVisibility
        {
            get { return _footerSelectAllVisibility; }
            set
            {
                _footerSelectAllVisibility = value;
                OnPropertyChanged(() => FooterSelectAllVisibility);
            }
        }

        private Visibility _chordSelectorVisiblity = Visibility.Collapsed;
        public Visibility ChordSelectorVisibility
        {
            get { return _chordSelectorVisiblity; }
            set
            {
                _chordSelectorVisiblity = value;
                OnPropertyChanged(() => ChordSelectorVisibility);
            }
        }

        private Visibility _insertMarkerVisiblity = Visibility.Collapsed;
        public Visibility InsertMarkerVisiblity
        {
            get { return _insertMarkerVisiblity; }
            set
            {
                _insertMarkerVisiblity = value;
                OnPropertyChanged(() => InsertMarkerVisiblity);
            }
        }

        private Visibility _collaborationFooterVisible = Visibility.Collapsed;
        public Visibility CollaborationFooterVisible
        {
            get { return _collaborationFooterVisible; }
            set
            {
                _collaborationFooterVisible = value;
                OnPropertyChanged(() => CollaborationFooterVisible);
            }
        }

        private Visibility _editingFooterVisible = Visibility.Collapsed;
        public Visibility EditingFooterVisible
        {
            get { return _editingFooterVisible; }
            set
            {
                _editingFooterVisible = value;
                OnPropertyChanged(() => EditingFooterVisible);
            }
        }

        #endregion

        #region MeasureBar Methods, Properties, Commands and EventHandlers

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeaveBarCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeaveBarCommand
        {
            get
            {
                return _mouseLeaveBarCommand;
            }
            set
            {
                if (value != _mouseLeaveBarCommand)
                {
                    _mouseLeaveBarCommand = value;
                    OnPropertyChanged(() => MouseLeaveBarCommand);
                }
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
                EA.GetEvent<UpdateMeasureBarColor>().Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarForeground));
                EditorState.IsOverBar = false;
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterBarCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterBarCommand
        {
            get
            {
                return _mouseEnterBarCommand;
            }
            set
            {
                if (value != _mouseEnterBarCommand)
                {
                    _mouseEnterBarCommand = value;
                    OnPropertyChanged(() => MouseEnterBarCommand);
                }
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
                EA.GetEvent<UpdateMeasureBarColor>().Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarSelectorColor));
                EditorState.IsOverBar = true;
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpBarCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpBarCommand
        {
            get
            {
                return _mouseLeftButtonUpBarCommand;
            }
            set
            {
                if (value != _mouseLeftButtonUpBarCommand)
                {
                    _mouseLeftButtonUpBarCommand = value;
                    OnPropertyChanged(() => MouseLeftButtonUpBarCommand);
                }
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
                _mouseY = -1;
                _mouseX = -1;
                var parentStaff = (from a in Cache.Staffs where a.Id == Measure.Staff_Id select a).First();
                var parentStaffgroup = (from a in Cache.Staffgroups where a.Id == parentStaff.Staffgroup_Id select a).First();
                var payload =
                    new MeasureWidthChangePayload
                    {
                        Id = Measure.Id,
                        Sequence = Measure.Sequence,
                        Width = (int)(Width - (int)(_measureBarBeforeDragX - _measureBarAfterDragX)),
                        StaffgroupId = parentStaffgroup.Id
                    };

                EA.GetEvent<ResizeMeasure>().Publish(payload);
                _initializedWidth = Width;
                EditorState.IsResizingMeasure = false;
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownBarCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownBarCommand
        {
            get { return _mouseLeftButtonDownBarCommand; }
            set
            {
                _mouseLeftButtonDownBarCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownBarCommand);
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
                _mouseY = args.GetPosition(null).Y;
                _mouseX = args.GetPosition(null).X;
                _measureBarBeforeDragX = _mouseX;
                _isMouseCaptured = true;
                item.CaptureMouse();
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseRightButtonUpCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseRightButtonUpCommand
        {
            get { return _mouseRightButtonUpCommand; }
            set
            {
                if (value != _mouseRightButtonUpCommand)
                {
                    _mouseRightButtonUpCommand = value;
                    OnPropertyChanged(() => MouseRightButtonUpCommand);
                }
            }
        }

        public void GetNextPasteTarget()
        {
            //if the content of the clipboard is greater than the remaining s'space' in the target _measure, then
            //this method is called to help determine what _measure the paste should continue in.
            int index = Measure.Index;
            var measure = (from a in Cache.Measures where a.Index == index + 1 select a).DefaultIfEmpty(null).Single();
            EA.GetEvent<BroadcastNewMeasureRequest>().Publish(measure);
        }

        public void OnPopEditPopupMenu(Guid id)
        {
            if (id == Measure.Id)
            {
                var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset, MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
                //var pt = new Point(MeasureClick_X + 10, MeasureClick_Y + 10);
                var payload =
                    new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index, Measure.Index * DurationManager.BPM, DurationManager.BPM, Measure.Width, Measure.Staff_Id);

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
            var pt = new Point(MeasureClick_X + 10 - CompositionManager.XScrollOffset, MeasureClick_Y + 10 - CompositionManager.YScrollOffset);
            var payload =
                new Tuple<Point, int, int, double, double, string, Guid>(pt, Measure.Sequence, Measure.Index, Measure.Index * DurationManager.BPM, DurationManager.BPM, Measure.Width, Measure.Staff_Id);

            EA.GetEvent<SetEditPopupMenu>().Publish(payload);
            EA.GetEvent<UpdateEditPopupMenuTargetMeasure>().Publish(this);
            EA.GetEvent<UpdateEditPopupMenuItemsEnableState>().Publish(string.Empty);
            EA.GetEvent<ShowEditPopupMenu>().Publish(string.Empty);

            SetChordContext();
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveBarCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveBarCommand
        {
            get { return _mouseMoveBarCommand; }
            set
            {
                if (value != _mouseMoveBarCommand)
                {
                    _mouseMoveBarCommand = value;
                    OnPropertyChanged(() => MouseMoveBarCommand);
                }
            }
        }

        public void OnMouseMoveBar(ExtendedCommandParameter commandParameter)
        {
            try
            {
                if (EditorState.IsPrinting) return;
                var item = (Path)commandParameter.Parameter;
                var e = (MouseEventArgs)commandParameter.EventArgs;
                if (_isMouseCaptured)
                {
                    BarBackground = Preferences.BarSelectorColor;
                    BarForeground = Preferences.BarSelectorColor;
                    var x = e.GetPosition(null).X;
                    var deltaH = x - _mouseX;
                    var newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);
                    EA.GetEvent<UpdateMeasureBarX>().Publish(new Tuple<Guid, double>(Measure.Id, Math.Round(newLeft, 0)));
                    EA.GetEvent<UpdateMeasureBarColor>().Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarSelectorColor));
                    item.SetValue(Canvas.LeftProperty, newLeft);
                    _mouseY = e.GetPosition(null).Y;
                    _mouseX = e.GetPosition(null).X;
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        public void OnResizeMeasure(object obj)
        {
            try
            {
                //this handler is basically a filter that weeds out the measures that should not be resized.
                //SetWidth() is called only on measures that should be resized
                EditorState.Ratio = 1;

                double startWidth = double.Parse(Measure.Width);
                var payload = (MeasureWidthChangePayload)obj;

                _Enum.MeasureResizeScope saveScope = EditorState.MeasureResizeScope;
                //TODO: with the hardcode MeasureResizeScope below we are bypassing the various MeasureResizeScopes in the 
                //switch block. I'm leaving all these scopes in place because I don't know if I'll use them in the future

                //Note: MeasureResizeScope cannot be 'Staff' when the StaffConfiguration is 'Grand'
                EditorState.MeasureResizeScope = _Enum.MeasureResizeScope.Composition;

                switch (EditorState.MeasureResizeScope)
                {
                    case _Enum.MeasureResizeScope.Staff:
                        //the width of the target _measure is set to the width specified by the user
                        if (payload.Id == Measure.Id)
                        {
                            if (EditorState.StaffConfiguration == _Enum.StaffConfiguration.MultiInstrument ||
                                EditorState.StaffConfiguration == _Enum.StaffConfiguration.Simple)
                            {
                                SetWidth(payload.Width);
                                EditorState.Ratio = payload.Width / startWidth;
                                AdjustContent();
                            }
                        }

                        break;
                    case _Enum.MeasureResizeScope.Staffgroup:
                        if (payload.Sequence == _measure.Sequence) //... this m is in the same staffgroup as the m m, and has the same seq as the m m
                        {
                            try
                            {
                                //the width of every _measure in the this particular measures staffgroup is set 
                                //to the width specified by the user on the target _measure
                                Staff staff = (from a in Cache.Staffs
                                               where a.Id == _measure.Staff_Id
                                               select a).First();

                                var staffgroup = (from a in Cache.Staffgroups
                                                  where a.Id == staff.Staffgroup_Id
                                                  select a).DefaultIfEmpty(null).Single();

                                if (payload.StaffgroupId == staffgroup.Id)
                                {
                                    SetWidth(payload.Width);
                                }
                            }
                            catch
                            {
                                //TODO
                                //we are swallowing a error here. measures created for the new composition dialog are
                                //still present somehow. but their parent staff is not, so _measure.Staff_Id points to nothing
                                //fix this in DetachNewCompositionPanelComposition. we are detaching the measures from the repository,
                                //but unity still 'sees' the _measure.
                            }
                            AdjustContent();
                        }
                        break;
                    case _Enum.MeasureResizeScope.Composition:
                        //the width of every _measure in the composition with the same seq is set to the width specified by the user on the target _measure.
                        if (payload.Sequence == _measure.Sequence)
                        {
                            SetWidth(payload.Width);
                            AdjustContent();
                        }
                        break;
                    case _Enum.MeasureResizeScope.Global:
                        //the width of every _measure in the composition is set to the width specified by the user on the target _measure.
                        SetWidth(payload.Width);
                        AdjustContent();
                        break;
                }
                EA.GetEvent<DeselectAllBars>().Publish(string.Empty);
                EditorState.MeasureResizeScope = saveScope;
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
                _Enum.MeasureArrangeMode currentAction = Preferences.MeasureArrangeMode;
                if (ActiveChords.Count > 0)
                {
                    if (MeasureManager.IsPackedMeasure(Measure))
                    {
                        Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.ManualResizePacked;
                        EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                        Preferences.MeasureArrangeMode = currentAction;
                    }
                    else
                    {
                        Preferences.MeasureArrangeMode = _Enum.MeasureArrangeMode.ManualResizeNotPacked;
                        EA.GetEvent<ArrangeMeasure>().Publish(Measure);
                        var chord = (from c in ActiveChords select c).OrderBy(p => p.StartTime).Last();
                        if (chord.Location_X + Preferences.MeasureMaximumEditingSpace > Width)
                        {
                            AdjustTrailingSpace(Measure.Id, Preferences.MeasureMaximumEditingSpace);
                        }
                        Preferences.MeasureArrangeMode = currentAction;
                    }
                }
                var staff = (from a in Cache.Staffs
                             where a.Id == _measure.Staff_Id
                             select a).DefaultIfEmpty(null).Single();

                if (staff != null) //TODO: (staff == null) should never happen. But, right now, some objects created by 
                //NewCompositionPanelViewModel, are not getting purged properly.
                {
                    double staffWidth = (from a in staff.Measures select double.Parse(a.Width)).Sum() + Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin - 70;
                    EditorState.GlobalStaffWidth = staffWidth;
                    EA.GetEvent<SetProvenanceWidth>().Publish(staffWidth);
                }
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }
        }

        private void SetWidth(double width)
        {
            _widthChangeRatio = 1;
            if (!EditorState.IsOpening)
            {
                _widthChangeRatio = width / Width * _baseRatio;
                _baseRatio = _widthChangeRatio;
            }
            Width = (int)Math.Floor(width);
        }

        private int _measureBarX;
        public int MeasureBar_X
        {
            get { return _measureBarX; }
            set
            {
                _measureBarX = value;
                OnPropertyChanged(() => MeasureBar_X);
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                //it was possible to drag a bar to the left of the preceding bar which produced a negative width.
                if (_width < 40)
                    _width = 40;
                Measure.Width = value.ToString(CultureInfo.InvariantCulture);
                MeasureBar_X = 0;
                OnPropertyChanged(() => Width);
            }
        }

        private string _verseMargin;
        public string VerseMargin
        {
            get { return _verseMargin; }
            set
            {
                _verseMargin = value;
                OnPropertyChanged(() => VerseMargin);
            }
        }

        private string _topMarkerMargin;
        public string TopMarkerMargin
        {
            get { return _topMarkerMargin; }
            set
            {
                _topMarkerMargin = value;
                OnPropertyChanged(() => TopMarkerMargin);
            }
        }

        private string _topInsertMarkerMargin;
        public string TopInsertMarkerMargin
        {
            get { return _topInsertMarkerMargin; }
            set
            {
                _topInsertMarkerMargin = value;
                OnPropertyChanged(() => TopInsertMarkerMargin);
            }
        }

        private string _topMarkerLabelMargin;
        public string TopMarkerLabelMargin
        {
            get { return _topMarkerLabelMargin; }
            set
            {
                _topMarkerLabelMargin = value;
                OnPropertyChanged(() => TopMarkerLabelMargin);
            }
        }

        private string _topInsertMarkerLabelMargin;
        public string TopInsertMarkerLabelMargin
        {
            get { return _topInsertMarkerLabelMargin; }
            set
            {
                _topInsertMarkerLabelMargin = value;
                OnPropertyChanged(() => TopInsertMarkerLabelMargin);
            }
        }

        private string _bottomChordSelectorMargin;
        public string BottomMarkerMargin
        {
            get { return _bottomChordSelectorMargin; }
            set
            {
                _bottomChordSelectorMargin = value;
                OnPropertyChanged(() => BottomMarkerMargin);
            }
        }

        private string _bottomInsertMarkerMargin;
        public string BottomInsertMarkerMargin
        {
            get { return _bottomInsertMarkerMargin; }
            set
            {
                _bottomInsertMarkerMargin = value;
                OnPropertyChanged(() => BottomInsertMarkerMargin);
            }
        }

        private string _markerLabelPath = string.Empty;
        public string MarkerLabelPath
        {
            get { return _markerLabelPath; }
            set
            {
                _markerLabelPath = value;
                OnPropertyChanged(() => MarkerLabelPath);
            }
        }

        private string _insertMarkerLabelPath = string.Empty;
        public string InsertMarkerLabelPath
        {
            get { return _insertMarkerLabelPath; }
            set
            {
                _insertMarkerLabelPath = value;
                OnPropertyChanged(() => InsertMarkerLabelPath);
            }
        }

        private string _markerColor = string.Empty;
        public string MarkerColor
        {
            get { return _markerColor; }
            set
            {
                _markerColor = value;
                OnPropertyChanged(() => MarkerColor);
            }
        }

        private string _insertMarkerColor = string.Empty;
        public string InsertMarkerColor
        {
            get { return _insertMarkerColor; }
            set
            {
                _insertMarkerColor = value;
                OnPropertyChanged(() => InsertMarkerColor);
            }
        }

        public struct MeasureWidthChangePayload
        {
            public Guid Id;
            public int Sequence;
            public int Width;
            public Guid StaffgroupId;

            public MeasureWidthChangePayload(Guid id, int sequence, int width, Guid staffgroupId)
            {
                Id = id;
                Sequence = sequence;
                Width = width;
                StaffgroupId = staffgroupId;
            }
        }

        #endregion
    }
}