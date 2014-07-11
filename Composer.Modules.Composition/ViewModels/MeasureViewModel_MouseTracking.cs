using System.Linq;
using System.Windows;
using System.Windows.Input;
using Composer.Infrastructure;
using Composer.Infrastructure.Behavior;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.Views;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class MeasureViewModel
    {
        private CompositionView _compositionView;
        private string _coordinates;
        private Visibility _cursorVisible = Visibility.Collapsed;
        private int _cursorX;
        private int _cursorY;
        private Visibility _ledgerGuideVisible = Visibility.Collapsed;
        private Visibility _insertMarkerVisiblity = Visibility.Collapsed;
        private int _ledgerGuideX;
        private int _ledgerGuideY;
        private int _measureClickX;
        private int _measureClickY;
        private string _bottomInsertMarkerMargin;
        private string _insertMarkerColor = string.Empty;
        private string _insertMarkerLabelPath = string.Empty;
        private string _markerColor = string.Empty;
        private string _markerLabelPath = string.Empty;
        private string _topInsertMarkerLabelMargin;
        private string _topInsertMarkerMargin;
        private string _topMarkerLabelMargin;
        private string _topMarkerMargin;
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

        public void HideCursor()
        {
            CursorVisible = Visibility.Collapsed;
        }

        private void HideMarker()
        {
            ChordSelectorVisibility = Visibility.Collapsed;
        }

        private void HideInsertMarker()
        {
            InsertMarkerVisiblity = Visibility.Collapsed;
        }

        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            if (EditorState.IsNewCompositionPanel) return;
            Coordinates = string.Format("{0}, {1}", _measureClickX, _measureClickY);

            //TODO: We're updating SpanManager every time the mouse moves ? really ?
            //UpdateSpanManager(); //commented out on 6/25/2014

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
                SetInsertMarkerAttributes(topY, bottomY);
            }
        }

        private void SetInsertMarkerAttributes(int topY, int bottomY)
        {
            TopInsertMarkerLabelMargin = topY < 5
                ? string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY + 98, 0, 0)
                : string.Format("{0},{1},{2},{3}", MeasureClick_X + 10, topY - 4, 0, 0);

            BottomInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X + 5, bottomY + 69, 0, 0);
            TopInsertMarkerMargin = string.Format("{0},{1},{2},{3}", MeasureClick_X - 3, topY + 14, 0, 0);

            InsertMarkerLabelPath = EditorState.IsRest() ? _insertRestPath : _insertNotePath;
            InsertMarkerColor = "Blue";
            InsertMarkerVisiblity = ChordSelectorVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TrackChordMarker()
        {
            EditorState.Chord = null;
            HideMarker();
            EditorState.ReplacementMode = _Enum.ReplaceMode.None;
            if (EditorState.IsSaving) return;
            foreach (var cH in ActiveChords)
            {
                if (MeasureClick_X > cH.Location_X + 14 && MeasureClick_X < cH.Location_X + 22)
                {
                    HideLedgerGuide();
                    EditorState.Chord = cH;
                    var topY = cH.Notes[0].Location_Y;
                    var bottomY = cH.Notes[0].Location_Y;
                    var ns = ChordManager.GetActiveNotes(cH.Notes);
                    foreach (var n in ns)
                    {
                        if (n.Location_Y < topY) topY = n.Location_Y;
                        if (n.Location_Y > bottomY) bottomY = n.Location_Y;
                    }
                    SetChordMarkerAttributes(cH.Location_X, ns[0].Type, topY, bottomY);
                }
            }
        }

        private void SetChordMarkerAttributes(int chX, short noteType, int topY, int bottomY)
        {
            if (noteType % 2 == 0 && EditorState.IsRest())
            {
                MarkerLabelPath = _replaceNoteWithRestPath;
                MarkerColor = "Red";
                EditorState.ReplacementMode = _Enum.ReplaceMode.Rest;
            }
            else if (noteType % 3 == 0 && !EditorState.IsRest())
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
                ? string.Format("{0},{1},{2},{3}", chX + 25, topY + 106, 0, 0)
                : string.Format("{0},{1},{2},{3}", chX + 25, topY - 4, 0, 0);
            BottomMarkerMargin = string.Format("{0},{1},{2},{3}", chX + 18, bottomY + 65, 0, 0);
            TopMarkerMargin = string.Format("{0},{1},{2},{3}", chX + 10, topY + 14, 0, 0);
            ChordSelectorVisibility = Visibility.Visible;
            InsertMarkerVisiblity = Visibility.Collapsed;
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


        public Visibility InsertMarkerVisiblity
        {
            get { return _insertMarkerVisiblity; }
            set
            {
                _insertMarkerVisiblity = value;
                OnPropertyChanged(() => InsertMarkerVisiblity);
            }
        }
    }
}
