using System;
using System.Globalization;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using System.Windows;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Behavior;
using System.Windows.Input;
using Composer.Modules.Composition.ViewModels.Helpers;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository;
using System.Collections.Generic;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class ArcViewModel : BaseViewModel, IArcViewModel
    {
        private static DataServiceRepository<Repository.DataService.Composition> _repository;

        private string cw = _Enum.ArcSweepDirection.Clockwise.ToString();
        private string ccw = _Enum.ArcSweepDirection.Counterclockwise.ToString();

        private bool _isSameStaff;
        private bool _selected;

        private int _locX1, _locY1, _locX2, _locY2;
        private double _deltaX, _deltaY;
        private Point _point1, _point2;
        _Enum.Orientation _stem1, _stem2;

        private Repository.DataService.Arc _arc;
        public Repository.DataService.Arc Arc
        {
            get { return _arc; }
            set
            {
                _arc = value;
                OnPropertyChanged(() => Arc);
                SetTieDuration();
            }
        }

        private Repository.DataService.Staff _startStaff;
        private Repository.DataService.Staff _endState;
        private Repository.DataService.Measure _startMeasure;
        private Repository.DataService.Measure _endMeasure;
        private Repository.DataService.Chord _startChord;
        private Repository.DataService.Chord _endChord;
        private Repository.DataService.Note _startNote;
        private Repository.DataService.Note _endNote;

        private double _opacity = Preferences.ArcOpacity;
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                OnPropertyChanged(() => Opacity);
            }
        }

        private string _centerX;
        public string CenterX
        {
            get { return _centerX; }
            set
            {
                _centerX = value;
                OnPropertyChanged(() => CenterX);
            }
        }

        private string _centerY;
        public string CenterY
        {
            get { return _centerY; }
            set
            {
                _centerY = value;
                OnPropertyChanged(() => CenterY);
            }
        }

        private string _startPoint;
        public string StartPoint
        {
            get { return _startPoint; }
            set
            {
                _startPoint = value;
                OnPropertyChanged(() => StartPoint);
            }
        }

        private string _size;
        public string Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged(() => Size);
            }
        }

        private string _flareSize;
        public string FlareArcSize
        {
            get { return _flareSize; }
            set
            {
                _flareSize = value;
                OnPropertyChanged(() => FlareArcSize);
            }

        }

        private double _angle;
        public double Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                OnPropertyChanged(() => Angle);
            }
        }

        private string _rotationAngle;
        public string RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                _rotationAngle = value;
                OnPropertyChanged(() => RotationAngle);
            }
        }

        private string _sweepDirection;
        public string SweepDirection
        {
            get { return _sweepDirection; }
            set
            {
                _sweepDirection = value;
                OnPropertyChanged(() => SweepDirection);
            }
        }

        private string _flareSweepDirection;
        public string FlareSweepDirection
        {
            get { return _flareSweepDirection; }
            set
            {
                _flareSweepDirection = value;
                OnPropertyChanged(() => FlareSweepDirection);
            }
        }

        private string _point;
        public string Point
        {
            get { return _point; }
            set
            {
                _point = value;
                OnPropertyChanged(() => Point);
            }
        }

        private string _isLargeArc;
        public string IsLargeArc
        {
            get { return _isLargeArc; }
            set
            {
                _isLargeArc = value;
                OnPropertyChanged(() => IsLargeArc);
            }
        }

        private double? _top;
        public double? Top
        {
            get { return _top; }
            set
            {
                _top = value;
                OnPropertyChanged(() => Top);
            }
        }

        private double _left;
        public double Left
        {
            get { return _left; }
            set
            {
                if (Math.Abs(_left - value) > 0)
                {
                    _left = value;
                    OnPropertyChanged(() => Left);
                }
            }
        }

        private string _background = Preferences.ArcBackground;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string _foreground = Preferences.ArcForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Foreground);
            }
        }

        private string _strokeWidth = Preferences.ArcStrokeThickness.ToString(CultureInfo.InvariantCulture);
        public string StrokeWidth
        {
            get { return _strokeWidth; }
            set
            {
                _strokeWidth = value;
                OnPropertyChanged(() => StrokeWidth);
            }
        }

        private void LoadProperties()
        {
            try
            {
                _startMeasure = (from a in Cache.Measures where a.Id == _startChord.Measure_Id select a).SingleOrDefault();
                _endMeasure = (from a in Cache.Measures where a.Id == _endChord.Measure_Id select a).SingleOrDefault();
                _startStaff = (from a in Cache.Staffs where a.Id == _startMeasure.Staff_Id select a).SingleOrDefault();
                _endState = (from a in Cache.Staffs where a.Id == _endMeasure.Staff_Id select a).SingleOrDefault();
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
            }

            if (_startStaff != null) if (_endState != null) _isSameStaff = _startStaff.Id != _endState.Id;
            if (_isSameStaff)
            {
                EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                //TODO show message:
                //At this time, slurs and ties cannot be drawn when the 2 notes are on different staffs. 
                //however, audibly, ties are handled properly;
            }
            else
            {
                if (EditorState.IsComposing)
                {
                    EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                }

                _stem1 = (_Enum.Orientation)_startNote.Orientation;
                _stem2 = (_Enum.Orientation)_endNote.Orientation;

                _locX1 = _startChord.Location_X;
                _locY1 = _startNote.Location_Y;
                _locX2 = _endChord.Location_X;
                _locY2 = _endNote.Location_Y;

                if (_stem1 != _stem2)
                {
                    _locY2 = _locY2 + ((_stem1 == _Enum.Orientation.Up) ? 25 : -25);
                }

                var point1 = Infrastructure.Support.Utilities.CoordinateSystem.TranslateCoordinatesToCompositionCoordinates(_startStaff.Id, _startMeasure.Sequence, _startMeasure.Index, _locX1, _locY1);
                var point2 = Infrastructure.Support.Utilities.CoordinateSystem.TranslateCoordinatesToCompositionCoordinates(_endState.Id, _endMeasure.Sequence, _endMeasure.Index, _locX2, _locY2);

                int fineTuneX = -37;
                int fineTuneY = 0;

                var xOffset = Defaults.StaffDimensionWidth + Defaults.CompositionLeftMargin + fineTuneX;

                _point1 = new Point(point1.X + fineTuneX, _locY1 + fineTuneY);
                _point2 = new Point(point2.X + fineTuneX, _locY2 + fineTuneY);

                _deltaX = Math.Abs(_point1.X - _point2.X);
                _deltaY = Math.Abs(_point1.Y - _point2.Y);
            }
        }

        private void SetTieDuration()
        {
            if (Arc.Type != (short)_Enum.ArcType.Tie || _endNote.Duration <= 0) return;

            var payload = new Tuple<Guid, decimal>(Arc.Note_Id1, _startNote.Duration + _endNote.Duration);
            EA.GetEvent<UpdateNoteDuration>().Publish(payload);

            payload = new Tuple<Guid, decimal>(Arc.Note_Id2, 0);
            EA.GetEvent<UpdateNoteDuration>().Publish(payload);
        }

        private int? _rotationDirection { get; set; }
        private int _stemDirectionOffset { get; set; }

        public void OnRender(Guid id)
        {
            LoadProperties();

            if (_rotationDirection == null) _rotationDirection = (_point1.Y >= _point2.Y) ? -1 : 1;
            if (_stemDirectionOffset == 0) _stemDirectionOffset = (_stem1 == _Enum.Orientation.Down) ? 50 : (_rotationDirection == 1) ? 80 : 65;

            //Bindable properties
            Angle = Math.Round((double)(Math.Atan2(_deltaY, _deltaX) * _rotationDirection) * (180 / Math.PI), 2);

            CenterX = string.Format("{0}", 0);
            CenterY = string.Format("{0}", 0);
            StartPoint = string.Format("{0},{1}", 0, 0);
            Point = string.Format("{0},{1}", _point2.X - _point1.X, 0);
            Size = string.Format("{0},{1}", (int)Math.Round(_deltaX / 1), Preferences.ArcDefaultDepth);

            //arcs are actually made up of 2 arc segments with the same endpoints for a tapered look. 
            //ArcFlareSize basically adjust how fat the arc is in the middle
            FlareArcSize = string.Format("{0},{1}", (int)Math.Round(_deltaX / 1), Preferences.ArcDefaultDepth + Preferences.ArcFlareSize);

            //the arc is drawn horizontal, then rotated around its center to the appropriate angle.
            RotationAngle = string.Format("{0}", 0);

            //IsLargeArc: http://msdn.microsoft.com/en-us/library/bb980166(v=vs.95).aspx
            IsLargeArc = "False";

            var left = _point1.X - 61;
            if (EditorState.AccidentalCount > 1)
            {
                left = left + (EditorState.AccidentalCount - 1) * EditorState.AccidentalWidth;
            }
            Left = left;

            //the following 3 arc proerties cannot change when a measure is arranged - for example, after a measure 
            //is resized. if they are allowed to change, an arc that has been flipped by the author will no longer 
            //be flipped. it will revert to its original (default) orientation. that's why we have the null checks.

            if (string.IsNullOrEmpty(SweepDirection)) SweepDirection = (_stem1 == _Enum.Orientation.Down) ? cw : ccw;

            if (string.IsNullOrEmpty(FlareSweepDirection)) FlareSweepDirection = (_stem1 == _Enum.Orientation.Down) ? ccw : cw;

            if (Top == null || Top == 0)
            {
                Top = _point1.Y + _stemDirectionOffset;
            }
            Arc.X1 = 0;
            Arc.Y1 = 0;
            Arc.X2 = (short)(_point2.X - _point1.X);
            Arc.Y2 = (short)_point2.Y;
            Arc.Top = (double)Top;
            Arc.Left = Left;
            Arc.Angle = Angle;
            Arc.ArcSweep = SweepDirection;
            Arc.FlareSweep = FlareSweepDirection;
        }

        public ArcViewModel(string id)
        {
            HideSelector();

            var c = (from obj in CompositionManager.Composition.Arcs where obj.Id == Guid.Parse(id) select obj);
            var e = c as List<Repository.DataService.Arc> ?? c.ToList();
            if (e.Any())
            {
                Arc = c.First();
            }

            if (Arc != null)
            {
                _startNote = (from a in Cache.Notes where a.Id == Arc.Note_Id1 select a).SingleOrDefault();
                _endNote = (from a in Cache.Notes where a.Id == Arc.Note_Id2 select a).SingleOrDefault();
                _startChord = (from a in Cache.Chords where a.Id == Arc.Chord_Id1 select a).SingleOrDefault();
                _endChord = (from a in Cache.Chords where a.Id == Arc.Chord_Id2 select a).SingleOrDefault();

                SweepDirection = Arc.ArcSweep;
                FlareSweepDirection = Arc.FlareSweep;
                Top = Arc.Top;
                DefineCommands();
                SubscribeEvents();
                _stemDirectionOffset = 0;
                _rotationDirection = null;
                EA.GetEvent<RenderArc>().Publish(Arc.Id);
                EA.GetEvent<UpdateArc>().Publish(string.Empty);
                this.EditControlsVisibility = Visibility.Collapsed;
            }
        }

        private ICommand _deleteArcCommand;
        public ICommand DeleteArcCommand
        {
            get { return _deleteArcCommand; }
            set
            {
                if (value != _deleteArcCommand)
                {
                    _deleteArcCommand = value;
                    OnPropertyChanged(() => DeleteArcCommand);
                }
            }
        }

        private ICommand _flipArcCommand;
        public ICommand FlipArcCommand
        {
            get { return _flipArcCommand; }
            set
            {
                if (value != _flipArcCommand)
                {
                    _flipArcCommand = value;
                    OnPropertyChanged(() => FlipArcCommand);
                }
            }
        }

        public void OnArcSelected(object obj)
        {

        }

        public void DefineCommands()
        {
            DeleteArcCommand = new DelegatedCommand<object>(OnDeleteArc);
            FlipArcCommand = new DelegatedCommand<object>(OnFlipArc);
            MouseEnterCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnter, CanHandleMouseEnter);
            MouseLeaveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeave, CanHandleMouseLeave);
            MouseLeftButtonUpCommand = new DelegatedCommand<object>(OnMouseLeftButtonUpCommand, CanHandleMouseLeftButtonUp);
            MouseRightButtonDownCommand = new DelegatedCommand<object>(OnMouseRightButtonDownCommand, CanHandleMouseRightButtonDown);
        }

        private void OnDeleteArc(object obj)
        {
            EA.GetEvent<DeleteArc>().Publish(Arc.Id);
        }

        private void OnFlipArc(object obj)
        {
            EA.GetEvent<FlipArc>().Publish(Arc.Id);
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<UpdateArc>().Subscribe(OnUpdateArc);
            EA.GetEvent<ArcSelected>().Subscribe(OnArcSelected);
            EA.GetEvent<FlipArc>().Subscribe(OnFlipArc);
            EA.GetEvent<DeSelectAll>().Subscribe(OnDeselectAll);
            EA.GetEvent<RenderArc>().Subscribe(OnRender);
        }

        private Visibility _editControlsVisibility = Visibility.Collapsed;
        public Visibility EditControlsVisibility
        {
            get { return _editControlsVisibility; }
            set
            {
                _editControlsVisibility = value;
                OnPropertyChanged(() => EditControlsVisibility);
            }
        }

        private Visibility _arcVisibility = Visibility.Visible;
        public Visibility ArcVisibility
        {
            get { return _arcVisibility; }
            set
            {
                _arcVisibility = value;
                OnPropertyChanged(() => ArcVisibility);
            }
        }

        public void OnUpdateArc(object obj)
        {
            if (CollaborationManager.IsActive(_startChord) && CollaborationManager.IsActive(_endChord))
            {
                ArcVisibility = Visibility.Visible;
            }
            else
            {
                ArcVisibility = Visibility.Collapsed;
            }
        }

        public void OnFlipArc(Guid id)
        {
            if (Arc.Id != id) return;
            Top = (SweepDirection == cw) ? Top + Measure.NoteHeight : Top - Measure.NoteHeight;
            SweepDirection = (SweepDirection == cw) ? ccw : cw;
            FlareSweepDirection = (SweepDirection == cw) ? ccw : cw;
            Arc.ArcSweep = SweepDirection;
            Arc.FlareSweep = FlareSweepDirection;
            Arc.Top = (double)Top;
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
            _repository.Update(Arc);
        }

        public void OnDeselectAll(object obj)
        {
            _selected = false;
            Foreground = Preferences.ArcForeground;
            EditControlsVisibility = Visibility.Collapsed;
            StrokeWidth = Preferences.ArcStrokeThickness.ToString(CultureInfo.InvariantCulture);
            Infrastructure.Support.Selection.Arcs.Remove(Arc);
        }

        private void OnMouseLeftButtonUpCommand(object o)
        {
            if (_selected)
            {
                OnDeselectAll(null);
            }
            else
            {
                if (! EditorState.IsPrinting)
                {
                    _selected = true;
                    Foreground = Preferences.ArcSelectorColor;
                    EditControlsVisibility = Visibility.Visible;
                    Infrastructure.Support.Selection.Arcs.Add(Arc);
                }
            }
        }

        private ICommand _mouseLeftButonUpCommand;

        public ICommand MouseLeftButtonUpCommand
        {
            get { return _mouseLeftButonUpCommand; }
            set
            {
                if (value == _mouseLeftButonUpCommand) return;
                _mouseLeftButonUpCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpCommand);
            }
        }

        private void OnMouseRightButtonDownCommand(object o)
        {
            EditorState.IsOverArc = true;
            ArcManager.SelectedArcId = Arc.Id;
            EA.GetEvent<PopEditPopupMenu>().Publish(_startMeasure.Id);
        }

        private ICommand _mouseRightButtonDownCommand;

        public ICommand MouseRightButtonDownCommand
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

        public override bool CanHandleMouseEnter(object obj)
        {
            return EditorState.IsComposing && ! EditorState.IsPrinting && !_selected;
        }
        public override bool CanHandleMouseLeave(object obj)
        {
            return EditorState.IsComposing && !EditorState.IsPrinting && !_selected;
        }
        private bool CanHandleMouseLeftButtonUp(object obj)
        {
            return EditorState.IsComposing;
        }
        private bool CanHandleMouseRightButtonDown(object obj)
        {
            return EditorState.IsComposing;
        }
        public override void OnMouseEnter(ExtendedCommandParameter commandParameter)
        {
            //TODO: for some reason CanExecute() not working for MouseLeave and MouseEnter
            EditorState.IsOverArc = false;
            if (CanHandleMouseEnter(""))
            {
                Background = Preferences.ArcHighlightBackground;
                Foreground = Preferences.ArcSelectorColor;
                StrokeWidth = Preferences.ArcHighlightStrokeWidth;
            }
        }

        public override void OnMouseLeave(ExtendedCommandParameter commandParameter)
        {
            //TODO: for some reason CanExecute() not working for MouseLeave and MouseEnter
            EditorState.IsOverArc = false;
            if (CanHandleMouseLeave(""))
            {
                Background = Preferences.ArcBackground;
                Foreground = Preferences.ArcForeground;
                StrokeWidth = Preferences.ArcStrokeThickness.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}