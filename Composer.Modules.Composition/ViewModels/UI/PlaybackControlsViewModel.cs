using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Repository;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Linq;
using System.Windows;
using Composer.Infrastructure.Behavior;
using System.Windows.Browser;
using System.Collections.Generic;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public class PlaybackControlsViewModel : BaseViewModel, IPlaybackControlsViewModel, IEventing
    {
        public enum ButtonState
        {
            Default,
            Hovering,
            Selected
        }

        public enum ButtonType
        {
            Play,
            Pause,
            Stop
        }

        public PlaybackControlsViewModel(string targetId, string location)
        {
            SubscribeEvents();
            DefineCommands();

            if (string.IsNullOrEmpty(targetId))
            {
                //arriving here means this PlaybackControlsView was loaded into the PlaybackControlsRegion in Shell.xaml
                //by the CompositionModule. Everywhere else (MeasureView and HubView) the PlaybackControlsView is loaded
                //directly in the xaml markup with TargetId and Location as attributes.
                targetId = CompositionManager.Composition.Id.ToString();
                location = "Palette";
                this.ScaleX = 2.2;
                this.ScaleY = 2.2;
            }
            else
            {
                this.ScaleX = 1;
                this.ScaleY = 1;
            }
            TargetId = Guid.Parse(targetId);

            switch (location)
            {
                case "Hub":
                    ControlMargin = "5,0,0,0";
                    Location = _Enum.PlaybackInitiatedFrom.Hub;
                    break;
                case "Palette":
                    ControlMargin = "-4,2,0,0";
                    Location = _Enum.PlaybackInitiatedFrom.Palette;
                    break;
                case "Measure":
                    ControlMargin = "5,-3,0,0";
                    Location = _Enum.PlaybackInitiatedFrom.Measure;
                    break;
                default:
                    ControlMargin = "5,0,0,0";
                    Location = _Enum.PlaybackInitiatedFrom.Unknown;
                    break;
            }
            Id = Guid.NewGuid();
            PlaybackHelper.PlayackControlViewModels.Add(this);
        }

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
            }
        }

        private bool _isPaused = false;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
            }
        }

        private Guid _id = Guid.Empty;
        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        private _Enum.PlaybackInitiatedFrom _location = _Enum.PlaybackInitiatedFrom.Unknown;
        public _Enum.PlaybackInitiatedFrom Location
        {
            get { return _location; }
            set
            {
                _location = value;
            }
        }

        private Guid _targetId = Guid.Empty;
        public Guid TargetId
        {
            get { return _targetId; }
            set
            {
                _targetId = value;
                OnPropertyChanged(() => TargetId);
            }
        }

        private Microsoft.Practices.Composite.Events.IEventAggregator ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        public Microsoft.Practices.Composite.Events.IEventAggregator Ea
        {
            get { return ea; }
        }

        public Repository.DataServiceRepository<Repository.DataService.Composition> repository =
            ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();

        public Repository.DataServiceRepository<Repository.DataService.Composition> Repository
        {
            get { return repository; }
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<FinishedPlayback>().Subscribe(OnFinishedPlayback);
            EA.GetEvent<StopPlay>().Subscribe(OnStopPlay, true);
        }

        public void DefineCommands()
        {
            MouseLeavePlayCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeavePlay, null);
            MouseEnterPlayCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterPlay, null);
            MouseLeftButtonDownPlayCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnPlay, null);

            MouseLeavePauseCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeavePause, null);
            MouseEnterPauseCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterPause, null);
            MouseLeftButtonDownPauseCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnPause, null);

            MouseLeaveStopCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeaveStop, null);
            MouseEnterStopCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseEnterStop, null);
            MouseLeftButtonDownStopCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonDownOnStop, null);

            ClickPlayCommand = new DelegatedCommand<object>(OnClickPlay);
            ClickStopCommand = new DelegatedCommand<object>(OnClickStop);
            ClickPauseCommand = new DelegatedCommand<object>(OnClickPause);
        }

        private void SetButtonStyle(ButtonState state, ButtonType type)
        {
            switch (state)
            {
                case ButtonState.Default:
                    switch (type)
                    {
                        case ButtonType.Play:
                            this.PlayButtonColor = Preferences.PlaybackControlColor_Default;
                            this.PlayButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
                            this.PlayButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
                            break;
                        case ButtonType.Pause:
                            this.PauseButtonColor = Preferences.PlaybackControlColor_Default;
                            this.PauseButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
                            this.PauseButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
                            break;
                        case ButtonType.Stop:
                            this.StopButtonColor = Preferences.PlaybackControlColor_Default;
                            this.StopButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
                            this.StopButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
                            break;
                    }
                    break;
                case ButtonState.Hovering:
                    switch (type)
                    {
                        case ButtonType.Play:
                            this.PlayButtonColor = Preferences.PlaybackControlColor_Hover;
                            this.PlayButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Hover;
                            this.PlayButtonStroke = Preferences.PlaybackControlStrokeColor_Hover;
                            break;
                        case ButtonType.Pause:
                            this.PauseButtonColor = Preferences.PlaybackControlColor_Hover;
                            this.PauseButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Hover;
                            this.PauseButtonStroke = Preferences.PlaybackControlStrokeColor_Hover;
                            break;
                        case ButtonType.Stop:
                            this.StopButtonColor = Preferences.PlaybackControlColor_Hover;
                            this.StopButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Hover;
                            this.StopButtonStroke = Preferences.PlaybackControlStrokeColor_Hover;
                            break;
                    }
                    break;
                case ButtonState.Selected:
                    switch (type)
                    {
                        case ButtonType.Play:
                            this.PlayButtonColor = Preferences.PlaybackControlColor_Selected;
                            this.PlayButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Selected;
                            this.PlayButtonStroke = Preferences.PlaybackControlStrokeColor_Selected;
                            break;
                        case ButtonType.Pause:
                            this.PauseButtonColor = Preferences.PlaybackControlColor_Selected;
                            this.PauseButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Selected;
                            this.PauseButtonStroke = Preferences.PlaybackControlStrokeColor_Selected;
                            break;
                        case ButtonType.Stop:
                            this.StopButtonColor = Preferences.PlaybackControlColor_Selected;
                            this.StopButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Selected;
                            this.StopButtonStroke = Preferences.PlaybackControlStrokeColor_Selected;
                            break;
                    }
                    break;
            }
        }

        #region Styling

        private Double _playButtonScale = 1;
        public Double PlayButtonScale
        {
            get { return _playButtonScale; }
            set
            {
                _playButtonScale = value;
                OnPropertyChanged(() => PlayButtonScale);
            }
        }

        private Double _pauseButtonScale = 1;
        public Double PauseButtonScale
        {
            get { return _pauseButtonScale; }
            set
            {
                _pauseButtonScale = value;
                OnPropertyChanged(() => PauseButtonScale);
            }
        }

        private Double _stopButtonScale = 1;
        public Double StopButtonScale
        {
            get { return _stopButtonScale; }
            set
            {
                _stopButtonScale = value;
                OnPropertyChanged(() => StopButtonScale);
            }
        }

        private string _buttonMargin = "5,0,0,0";
        public string ButtonMargin
        {
            get { return _buttonMargin; }
            set
            {
                _buttonMargin = value;
                OnPropertyChanged(() => ButtonMargin);
            }
        }

        private string _controlMargin = "5,0,0,0";
        public string ControlMargin
        {
            get { return _controlMargin; }
            set
            {
                _controlMargin = value;
                OnPropertyChanged(() => ControlMargin);
            }
        }

        private Visibility _captionVisibility = Visibility.Collapsed;
        public Visibility CaptionVisibility
        {
            get { return _captionVisibility; }
            set
            {
                _captionVisibility = value;
                OnPropertyChanged(() => CaptionVisibility);
            }
        }

        private string _background = "Transparent";
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string _playButtonColor = Preferences.PlaybackControlColor_Default;
        public string PlayButtonColor
        {
            get { return _playButtonColor; }
            set
            {
                _playButtonColor = value;
                OnPropertyChanged(() => PlayButtonColor);
            }
        }

        private string _playButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
        public string PlayButtonStroke
        {
            get { return _playButtonStroke; }
            set
            {
                _playButtonStroke = value;
                OnPropertyChanged(() => PlayButtonStroke);
            }
        }

        private string _pauseButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
        public string PauseButtonStroke
        {
            get { return _pauseButtonStroke; }
            set
            {
                _pauseButtonStroke = value;
                OnPropertyChanged(() => PauseButtonStroke);
            }
        }

        private string _stopButtonStroke = Preferences.PlaybackControlStrokeColor_Default;
        public string StopButtonStroke
        {
            get { return _stopButtonStroke; }
            set
            {
                _stopButtonStroke = value;
                OnPropertyChanged(() => StopButtonStroke);
            }
        }

        private string _pauseButtonColor = Preferences.PlaybackControlColor_Default;
        public string PauseButtonColor
        {
            get { return _pauseButtonColor; }
            set
            {
                _pauseButtonColor = value;
                OnPropertyChanged(() => PauseButtonColor);
            }
        }

        private string _stopButtonColor = Preferences.PlaybackControlColor_Default;
        public string StopButtonColor
        {
            get { return _stopButtonColor; }
            set
            {
                _stopButtonColor = value;
                OnPropertyChanged(() => StopButtonColor);
            }
        }

        private double _playButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
        public double PlayButtonStrokeThickness
        {
            get { return _playButtonStrokeThickness; }
            set
            {
                _playButtonStrokeThickness = value;
                OnPropertyChanged(() => PlayButtonStrokeThickness);
            }
        }

        private double _pauseButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
        public double PauseButtonStrokeThickness
        {
            get { return _pauseButtonStrokeThickness; }
            set
            {
                _pauseButtonStrokeThickness = value;
                OnPropertyChanged(() => PauseButtonStrokeThickness);
            }
        }

        private double _stopButtonStrokeThickness = Preferences.PlaybackControlStrokeThickness_Default;
        public double StopButtonStrokeThickness
        {
            get { return _stopButtonStrokeThickness; }
            set
            {
                _stopButtonStrokeThickness = value;
                OnPropertyChanged(() => StopButtonStrokeThickness);
            }
        }

        private double _playButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
        public double PlayButtonOpacity
        {
            get { return _playButtonOpacity; }
            set
            {
                _playButtonOpacity = value;
                OnPropertyChanged(() => PlayButtonOpacity);
            }
        }

        private double _pauseButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
        public double PauseButtonOpacity
        {
            get { return _pauseButtonOpacity; }
            set
            {
                _pauseButtonOpacity = value;
                OnPropertyChanged(() => PauseButtonOpacity);
            }
        }

        private double _stopButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
        public double StopButtonOpacity
        {
            get { return _stopButtonOpacity; }
            set
            {
                _stopButtonOpacity = value;
                OnPropertyChanged(() => StopButtonOpacity);
            }
        }

        #endregion

        #region Handlers

        public void OnFinishedPlayback(object obj)
        {
            OnClickStop(string.Empty);
        }

        public void OnStopPlay(object obj)
        {
            //if (this.IsPlaying)
            //{
                OnClickStop(string.Empty);
            //}
        }

        public void OnClickPause(object obj)
        {
            if (EditorState.IsPlaying)
            {
                ResetPlaybackControls();
                SetButtonStyle(ButtonState.Selected, ButtonType.Pause);
                SetButtonStyle(ButtonState.Hovering, ButtonType.Play);
                if (Location != _Enum.PlaybackInitiatedFrom.Palette)
                {
                    this.PauseButtonScale = Preferences.PlaybackControlButtonScale_Selected;
                    this.PlayButtonScale = Preferences.PlaybackControlButtonScale_Selected;
                }
                this.PauseButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
                this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
                EditorState.IsPaused = true;
                EditorState.IsPlaying = false;
                this.IsPaused = true;
                this.IsPlaying = false;
                HtmlPage.Window.Invoke("PausePlayback");
            }
        }

        public void OnMouseLeavePause(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Default, ButtonType.Pause);
                this.PauseButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            }
        }

        public void OnMouseEnterPause(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Hovering, ButtonType.Pause);
                this.PauseButtonOpacity = 1;
            }
        }

        public void OnMouseLeftButtonDownOnPause(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Selected, ButtonType.Pause);
                this.PauseButtonOpacity = 1;
            }
        }

        public void OnClickStop(object obj)
        {
            ResetPlaybackControls();
            HtmlPage.Window.Invoke("StopPlayback");
            EditorState.ResumeStarttime = 0;
        }

        public void OnMouseLeaveStop(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying || EditorState.IsPaused) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Default, ButtonType.Stop);
                this.StopButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            }
        }

        public void OnMouseEnterStop(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying || EditorState.IsPaused) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Hovering, ButtonType.Stop);
                this.StopButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
            }
        }

        public void OnMouseLeftButtonDownOnStop(ExtendedCommandParameter commandParameter)
        {
            if ((EditorState.IsPlaying || EditorState.IsPaused) && EditorState.ActivePlaybackControlId == this.Id)
            {
                SetButtonStyle(ButtonState.Selected, ButtonType.Stop);
                this.StopButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
            }
        }

        public void OnClickPlay(object obj)
        {
            if (EditorState.IsPlaying || EditorState.IsPaused)
            {
                EA.GetEvent<StopPlay>().Publish(string.Empty);
            }
            ResetPlaybackControls();
            EditorState.ActivePlaybackControlId = this.Id;
            SetButtonStyle(ButtonState.Selected, ButtonType.Play);
            if (Location != _Enum.PlaybackInitiatedFrom.Palette)
            {
                this.PlayButtonScale = Preferences.PlaybackControlButtonScale_Selected;
            }
            this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
            EditorState.IsPlaying = true;
            this.IsPlaying = true;
            EA.GetEvent<HideMeasureEditHelpers>().Publish(string.Empty);
            switch (Location)
            {
                case _Enum.PlaybackInitiatedFrom.Hub:
                    EA.GetEvent<PlayCompositionFromHub>().Publish(this.TargetId);
                    break;
                case _Enum.PlaybackInitiatedFrom.Palette:
                    EA.GetEvent<PlayComposition>().Publish(_Enum.PlaybackInitiatedFrom.Palette);
                    break;
                case _Enum.PlaybackInitiatedFrom.Measure:
                    Location = _Enum.PlaybackInitiatedFrom.Measure;
                    var a = (from b in Cache.Measures where b.Id == TargetId select b);
                    var e = a as List<Repository.DataService.Measure> ?? a.ToList();
                    if (e.Any())
                    {
                        EA.GetEvent<PlayMeasure>().Publish(a.Single());
                    }
                    break;
            }
        }

        public void OnMouseLeavePlay(ExtendedCommandParameter commandParameter)
        {
            if (EditorState.ActivePlaybackControlId != this.Id && !this.IsPaused)
            {
                SetButtonStyle(ButtonState.Default, ButtonType.Play);
                this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            }
        }

        public void OnMouseEnterPlay(ExtendedCommandParameter commandParameter)
        {
            if (EditorState.ActivePlaybackControlId != this.Id && !this.IsPaused)
            {
                SetButtonStyle(ButtonState.Hovering, ButtonType.Play);
                this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
            }
        }

        public void OnMouseLeftButtonDownOnPlay(ExtendedCommandParameter commandParameter)
        {
            if (EditorState.ActivePlaybackControlId != this.Id && !this.IsPaused)
            {
                SetButtonStyle(ButtonState.Selected, ButtonType.Play);
                this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Enabled;
            }
        }

        private void ResetPlaybackControls()
        {
            SetButtonStyle(ButtonState.Default, ButtonType.Stop);
            SetButtonStyle(ButtonState.Default, ButtonType.Pause);
            SetButtonStyle(ButtonState.Default, ButtonType.Play);
            this.PlayButtonScale = Preferences.PlaybackControlButtonScale_Default;
            this.StopButtonScale = Preferences.PlaybackControlButtonScale_Default;
            this.PauseButtonScale = Preferences.PlaybackControlButtonScale_Default;
            this.StopButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            this.PlayButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            this.PauseButtonOpacity = Preferences.PlaybackControlOpacity_Disabled;
            EditorState.IsPlaying = false;
            EditorState.IsPaused = false;
            EditorState.ActivePlaybackControlId = Guid.Empty;
            this.IsPaused = false;
            this.IsPlaying = false;
        }
        #endregion

        #region Commands

        private DelegatedCommand<object> _clickPlayCommand;
        public DelegatedCommand<object> ClickPlayCommand
        {
            get { return _clickPlayCommand; }
            set
            {
                _clickPlayCommand = value;
                OnPropertyChanged(() => ClickPlayCommand);
            }
        }

        private DelegatedCommand<object> _clickPauseCommand;
        public DelegatedCommand<object> ClickPauseCommand
        {
            get { return _clickPauseCommand; }
            set
            {
                _clickPauseCommand = value;
                OnPropertyChanged(() => ClickPauseCommand);
            }
        }

        private DelegatedCommand<object> _clickStopCommand;
        public DelegatedCommand<object> ClickStopCommand
        {
            get { return _clickStopCommand; }
            set
            {
                _clickStopCommand = value;
                OnPropertyChanged(() => ClickStopCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeavePauseCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeavePauseCommand
        {
            get { return _mouseLeavePauseCommand; }
            set
            {
                _mouseLeavePauseCommand = value;
                OnPropertyChanged(() => MouseLeavePauseCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterPauseCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterPauseCommand
        {
            get { return _mouseEnterPauseCommand; }
            set
            {
                _mouseEnterPauseCommand = value;
                OnPropertyChanged(() => MouseEnterPauseCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownPauseCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownPauseCommand
        {
            get { return _mouseLeftButtonDownPauseCommand; }
            set
            {
                _mouseLeftButtonDownPauseCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownPauseCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeaveStopCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeaveStopCommand
        {
            get { return _mouseLeaveStopCommand; }
            set
            {
                _mouseLeaveStopCommand = value;
                OnPropertyChanged(() => MouseLeaveStopCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterStopCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterStopCommand
        {
            get { return _mouseEnterStopCommand; }
            set
            {
                _mouseEnterStopCommand = value;
                OnPropertyChanged(() => MouseEnterStopCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownStopCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownStopCommand
        {
            get { return _mouseLeftButtonDownStopCommand; }
            set
            {
                _mouseLeftButtonDownStopCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownStopCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeavePlayCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeavePlayCommand
        {
            get { return _mouseLeavePlayCommand; }
            set
            {
                _mouseLeavePlayCommand = value;
                OnPropertyChanged(() => MouseLeavePlayCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterPlayCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterPlayCommand
        {
            get { return _mouseEnterPlayCommand; }
            set
            {
                _mouseEnterPlayCommand = value;
                OnPropertyChanged(() => MouseEnterPlayCommand);
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonDownPlayCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonDownPlayCommand
        {
            get { return _mouseLeftButtonDownPlayCommand; }
            set
            {
                _mouseLeftButtonDownPlayCommand = value;
                OnPropertyChanged(() => MouseLeftButtonDownPlayCommand);
            }
        }

        #endregion
    }
}
