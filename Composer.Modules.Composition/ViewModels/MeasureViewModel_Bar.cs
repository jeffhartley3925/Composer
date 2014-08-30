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
        private int measureBarX;
        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseEnterBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeaveBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeftButtonDownBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeftButtonUpBarCommand;
        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseMoveBarCommand;
        private string barBackground = Preferences.BarBackground;
        private string barForeground = Preferences.BarForeground;
        private short barId;
        private string barMargin = "0";
        private double measureBarAfterDragX;
        private double measureBarBeforeDragX;

        private Visibility barVisibility = Visibility.Visible;
        public Visibility BarVisibility
        {
            get { return this.barVisibility; }
            set
            {
                this.barVisibility = value;
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
            get { return this.mouseLeaveBarCommand; }
            set
            {
                this.mouseLeaveBarCommand = value;
                OnPropertyChanged(() => MouseLeaveBarCommand);
            }
        }

        public short BarId
        {
            // TODO: BarId appears to be unused
            get { return this.barId; }
            set
            {
                this.barId = value;
                Measure.Bar_Id = this.barId;
                BarMargin = (from a in Bars.BarList where a.Id == this.barId select a.Margin).First();
                OnPropertyChanged(() => BarId);
            }
        }

        public string BarBackground
        {
            get { return this.barBackground; }
            set
            {
                this.barBackground = value;
                OnPropertyChanged(() => BarBackground);
            }
        }

        public string BarForeground
        {
            get { return this.barForeground; }
            set
            {
                this.barForeground = value;
                OnPropertyChanged(() => BarForeground);
            }
        }

        public string BarMargin
        {
            get { return this.barMargin; }
            set
            {
                this.barMargin = value;
                OnPropertyChanged(() => BarMargin);
            }
        }

        public int MeasureBarX
        {
            get { return this.measureBarX; }
            set
            {
                this.measureBarX = value;
                OnPropertyChanged(() => MeasureBarX);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterBarCommand
        {
            get { return this.mouseEnterBarCommand; }
            set
            {
                this.mouseEnterBarCommand = value;
                OnPropertyChanged(() => MouseEnterBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpBarCommand
        {
            get { return this.mouseLeftButtonUpBarCommand; }
            set
            {
                this.mouseLeftButtonUpBarCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownBarCommand
        {
            get { return this.mouseLeftButtonDownBarCommand; }
            set
            {
                this.mouseLeftButtonDownBarCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownBarCommand);
            }
        }

        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveBarCommand
        {
            get { return this.mouseMoveBarCommand; }
            set
            {
                this.mouseMoveBarCommand = value;
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
                if (!this.isMouseCaptured) return;
                BarBackground = Preferences.BarSelectorColor;
                BarForeground = Preferences.BarSelectorColor;
                var x = e.GetPosition(null).X;
                var deltaH = x - this.mouseX;
                var newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);
                EA.GetEvent<UpdateMeasureBarX>().Publish(new Tuple<Guid, double>(Measure.Id, Math.Round(newLeft, 0)));
                EA.GetEvent<UpdateMeasureBarColor>().Publish(new Tuple<Guid, string>(Measure.Id, Preferences.BarSelectorColor));
                item.SetValue(Canvas.LeftProperty, newLeft);
                this.mouseX = e.GetPosition(null).X;
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
                if (this.debugging)
                {
                    item.Stroke = new SolidColorBrush(Colors.Black);
                }
                this.mouseX = args.GetPosition(null).X;
                this.measureBarAfterDragX = this.mouseX;
                this.isMouseCaptured = false;
                item.ReleaseMouseCapture();
                this.mouseX = -1;
                var width = Width - (int)(this.measureBarBeforeDragX - this.measureBarAfterDragX);
                BroadcastWidthChange(width);
                this.initializedWidth = Width;
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

        public void OnUpdateMeasureBar(short bRiD)
        {
            // this event is broadcast to all measures. if this m has a end-bar with end-bar id = Bars.EndBarId, (if it 
            // is the last m in the last staffgroup), then it is reset to the bar id passed in.

            if (!EditorState.IsAddingStaffgroup) return;
            if (BarId == Bars.EndBarId)
            {
                BarId = bRiD;
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
                this.mouseX = args.GetPosition(null).X;
                this.measureBarBeforeDragX = this.mouseX;
                this.isMouseCaptured = true;
                item.CaptureMouse();
            }
        }
    }
}
