using System;
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

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class MeasureViewModel
    {
        private int _measureBarX;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeaveBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseMoveBarCommand;
        private string _barBackground = Preferences.BarBackground;
        private string _barForeground = Preferences.BarForeground;
        private short _barId;
        private string _barMargin = "0";
        private double _measureBarAfterDragX;
        private double _measureBarBeforeDragX;
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

        private void SubscribeBarEvents()
        {
            EA.GetEvent<UpdateMeasureBar>().Subscribe(OnUpdateMeasureBar);
            EA.GetEvent<SetMeasureEndBar>().Subscribe(OnSetMeasureEndBar);
            EA.GetEvent<DeselectAllBars>().Subscribe(OnDeselectAllBars);
        }
        private void DefineBarCommands()
        {
            MouseLeaveBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeaveBar, null);
            MouseEnterBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterBar, null);
            MouseLeftButtonUpBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(
                OnMouseLeftButtonUpOnBar, null);
            MouseLeftButtonDownBarCommand =
                new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnBar, null);
            MouseMoveBarCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMoveBar, null);
        }
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeaveBarCommand
        {
            get { return _mouseLeaveBarCommand; }
            set
            {
                _mouseLeaveBarCommand = value;
                OnPropertyChanged(() => MouseLeaveBarCommand);
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


        public string BarBackground
        {
            get { return _barBackground; }
            set
            {
                _barBackground = value;
                OnPropertyChanged(() => BarBackground);
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

        public int MeasureBarX
        {
            get { return _measureBarX; }
            set
            {
                _measureBarX = value;
                OnPropertyChanged(() => MeasureBarX);
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

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveBarCommand
        {
            get { return _mouseMoveBarCommand; }
            set
            {
                _mouseMoveBarCommand = value;
                OnPropertyChanged(() => MouseMoveBarCommand);
            }
        }

        public void OnUpdateMeasureBarX(Tuple<Guid, double> payload)
        {
            var m = Utils.GetMeasure(payload.Item1);
            if (m.Sequence != Measure.Sequence) return;
            try
            {
                MeasureBarX = int.Parse(payload.Item2.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Exceptions.HandleException(ex);
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

        public void OnDeselectAllBars(object obj)
        {
            BarForeground = Preferences.BarForeground;
            BarBackground = Preferences.BarBackground;
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
                var width = Width - (int)(_measureBarBeforeDragX - _measureBarAfterDragX);
                BroadcastWidthChange(width);
                _initializedWidth = Width;
                EditorState.IsResizingMeasure = false;
            }
        }

        private void BroadcastWidthChange(int wI)
        {
            var sG = Utils.GetStaffgroup(Measure);
            var widthChange =
                new WidthChange
                {
					MeasuregroupId = Mg.Id,
                    MeasureId = Measure.Id,
					MeasureIndex = Measure.Index,
                    StaffId = Measure.Staff_Id,
                    Sequence = Measure.Sequence,
                    Width = wI,
                    StaffgroupId = sG.Id
                };

            EA.GetEvent<ResizeSequence>().Publish(widthChange);
			EA.GetEvent<RespaceSequence>().Publish(new Tuple<Guid, int?>(Measure.Id, Measure.Sequence));
			EA.GetEvent<BumpSequenceWidth>().Publish(new Tuple<Guid, double?, int>(Measure.Id, wI, Measure.Sequence));
			EA.GetEvent<SetCompositionWidth>().Publish(Measure.Staff_Id);
        }

        public void OnSetMeasureEndBar(object obj)
        {
            try
            {
                if (Measure.Sequence != (Densities.MeasureDensity - 1) * Defaults.SequenceIncrement) return;
                var sG = Utils.GetStaffgroup(Measure);
                if (sG.Sequence != (Densities.StaffgroupDensity - 1) * Defaults.SequenceIncrement) return;
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
    }
}
