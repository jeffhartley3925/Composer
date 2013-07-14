using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Behavior;
using System.Collections.ObjectModel;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class TranspositionViewModel : BaseViewModel, ITranspositionViewModel
    {
        private List<TranspositionData> _transpositionLog;
        private TranspositionState _before;
        private TranspositionState _after;
        private string _locationValidation = string.Empty;
        private string _octaveValidation = string.Empty;
        private string _slotValidation = string.Empty;

        private int _deltaOctave;
        public int DeltaOctave
        {
            get { return _deltaOctave; }
            set
            {
                _deltaOctave = value;
                CanExecuteTranspose = IsTransposable();
            }
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                _interval = value;
                OnPropertyChanged(() => Interval);
            }
        }

        private int _rawInterval;
        public int RawInterval
        {
            get { return _rawInterval; }
            set
            {
                _rawInterval = value;
                OnPropertyChanged(() => RawInterval);
            }
        }

        private _Enum.Direction _direction;
        public _Enum.Direction Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                CanExecuteTranspose = IsTransposable();
                OnPropertyChanged(() => Direction);
            }
        }

        private _Enum.TranspositionMode _mode;
        public _Enum.TranspositionMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnPropertyChanged(() => Mode);
            }
        }

        public TranspositionViewModel()
        {
            DefineCommands();
            SubscribeEvents();
            CanExecuteCancel = true;
            CanExecuteTranspose = false;
            int keyId = (CompositionManager.Composition != null) ? CompositionManager.Composition.Key_Id : Infrastructure.Dimensions.Keys.Key.Id;
            CurrentKey = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Id == keyId select a).Single();
            SelectedKey = CurrentKey;
            Intervals = Infrastructure.Dimensions.Keys.IntervalList;
            SelectedInterval = (from a in Infrastructure.Dimensions.Keys.IntervalList where a.Name == Preferences.DefaultInterval select a).Single();
            Mode = _Enum.TranspositionMode.None;
        }

        private void DefineCommands()
        {
            TransposeButtonClickedCommand = new DelegateCommand<object>(OnTransposeButtonClicked, OnCanExecuteTranspose);
            CancelButtonClickedCommand = new DelegateCommand<object>(OnCancelButtonClicked, OnCanExecuteCancel);
            TransposeIntervalCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeIntervalCheckedCommand, null);
            TransposeIntervalUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeIntervalUncheckedCommand, null);
            TransposeOctaveCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeOctaveCheckedCommand, null);
            TransposeOctaveUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeOctaveUncheckedCommand, null);
            TransposeKeyCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeKeyCheckedCommand, null);
            TransposeKeyUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnTransposeKeyUncheckedCommand, null);
            IntervalUpUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnIntervalUpUncheckedCommand, null);
            IntervalUpCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnIntervalUpCheckedCommand, null);
            IntervalDownUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnIntervalDownUncheckedCommand, null);
            IntervalDownCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnIntervalDownCheckedCommand, null);
            OctaveUpUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnOctaveUpUncheckedCommand, null);
            OctaveUpCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnOctaveUpCheckedCommand, null);
            OctaveDownUncheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnOctaveDownUncheckedCommand, null);
            OctaveDownCheckedCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnOctaveDownCheckedCommand, null);
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition);
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            EditorState.AccidentalNotes = new List<string>();
            var state = (TranspositionState)payload.Item2;
            Infrastructure.Dimensions.Keys.Key = state.Key;
        }

        private Visibility _transpositionDetailsVisibility = Visibility.Visible;
        public Visibility TranspositionDetailsVisibility
        {
            get { return _transpositionDetailsVisibility; }
            set
            {
                _transpositionDetailsVisibility = value;
                OnPropertyChanged(() => TranspositionDetailsVisibility);
            }
        }

        private bool _keyTranspositionChecked;
        public bool KeyTranspositionChecked
        {
            get { return _keyTranspositionChecked; }
            set
            {
                _keyTranspositionChecked = value;
                Mode = (_keyTranspositionChecked) ? _Enum.TranspositionMode.Key : _Enum.TranspositionMode.None;
                OnPropertyChanged(() => KeyTranspositionChecked);

            }
        }

        private bool _intervalTranspositionChecked;
        public bool IntervalTranspositionChecked
        {
            get { return _intervalTranspositionChecked; }
            set
            {
                _intervalTranspositionChecked = value;
                Mode = (_intervalTranspositionChecked) ? _Enum.TranspositionMode.Interval : _Enum.TranspositionMode.None;
                OnPropertyChanged(() => IntervalTranspositionChecked);
            }
        }

        private bool _octaveTranspositionChecked;
        public bool OctaveTranspositionChecked
        {
            get { return _octaveTranspositionChecked; }
            set
            {
                _octaveTranspositionChecked = value;
                Mode = (_octaveTranspositionChecked) ? _Enum.TranspositionMode.Octave : _Enum.TranspositionMode.None;
                OnPropertyChanged(() => OctaveTranspositionChecked);
            }
        }

        private bool _octaveTranspositionEnabled;
        public bool OctaveTranspositionEnabled
        {
            get { return _octaveTranspositionEnabled; }
            set
            {
                _octaveTranspositionEnabled = value;
                if (_octaveTranspositionEnabled)
                {
                    KeyTranspositionEnabled = false;
                    KeyTranspositionChecked = false;
                    IntervalTranspositionEnabled = false;
                    IntervalTranspositionChecked = false;
                }
                OnPropertyChanged(() => OctaveTranspositionEnabled);
            }
        }

        private bool _intervalTranspositionEnabled;
        public bool IntervalTranspositionEnabled
        {
            get { return _intervalTranspositionEnabled; }
            set
            {
                _intervalTranspositionEnabled = value;
                if (_intervalTranspositionEnabled)
                {
                    KeyTranspositionEnabled = false;
                    KeyTranspositionChecked = false;
                    OctaveTranspositionEnabled = false;
                    OctaveTranspositionChecked = false;
                    DeltaOctave = 0;
                }
                OnPropertyChanged(() => IntervalTranspositionEnabled);
            }
        }

        private bool IsTransposable()
        {
            return (Mode == _Enum.TranspositionMode.Key && SelectedKey.Id != CurrentKey.Id) ||
                   (Mode == _Enum.TranspositionMode.Octave && DeltaOctave != 0) ||
                   (Mode == _Enum.TranspositionMode.Interval && Interval != 0 && Direction != _Enum.Direction.None);
        }

        private bool _keyTranspositionEnabled;
        public bool KeyTranspositionEnabled
        {
            get { return _keyTranspositionEnabled; }
            set
            {
                _keyTranspositionEnabled = value;
                if (!_keyTranspositionEnabled)
                {
                    CanExecuteTranspose = IsTransposable();
                }
                else
                {
                    IntervalTranspositionEnabled = false;
                    IntervalTranspositionChecked = false;
                    OctaveTranspositionEnabled = false;
                    OctaveTranspositionChecked = false;
                    DeltaOctave = 0;
                }
                OnPropertyChanged(() => KeyTranspositionEnabled);
            }
        }

        private string _targetKeyName;
        public string TargetKeyName
        {
            get { return _targetKeyName; }
            set
            {
                _targetKeyName = value;
                OnPropertyChanged(() => TargetKeyName);
            }
        }

        private Infrastructure.Dimensions.Key _currentKey;
        public Infrastructure.Dimensions.Key CurrentKey
        {
            get { return _currentKey; }
            set
            {
                _currentKey = value;
                OnPropertyChanged(() => CurrentKey);
            }
        }

        private List<Infrastructure.Dimensions.Interval> _intervals = Infrastructure.Dimensions.Keys.IntervalList;
        public List<Infrastructure.Dimensions.Interval> Intervals
        {
            get { return _intervals; }
            set
            {
                _intervals = value;
                OnPropertyChanged(() => Intervals);
            }
        }

        private List<Infrastructure.Dimensions.Key> _keys = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Listable select a).ToList();
        public List<Infrastructure.Dimensions.Key> Keys
        {
            get { return _keys; }
            set
            {
                _keys = value;
                OnPropertyChanged(() => Keys);
            }
        }

        private Infrastructure.Dimensions.Key _selectedKey;
        public Infrastructure.Dimensions.Key SelectedKey
        {
            get { return _selectedKey; }
            set
            {
                if (value != _selectedKey)
                {
                    _selectedKey = value;
                    TargetKeyName = _selectedKey.Name;
                    Interval = SelectedKey.Index - CurrentKey.Index;
                    CanExecuteTranspose = IsTransposable();
                    OnPropertyChanged(() => SelectedKey);
                }
            }
        }

        private Infrastructure.Dimensions.Interval _selectedInterval;
        public Infrastructure.Dimensions.Interval SelectedInterval
        {
            get { return _selectedInterval; }
            set
            {
                if (value != _selectedInterval)
                {
                    _selectedInterval = value;
                    OnPropertyChanged(() => SelectedInterval);
                }
            }
        }

        #region Transpose Button Support

        private bool _canExecuteTranspose;
        public bool CanExecuteTranspose
        {
            get { return _canExecuteTranspose; }
            set
            {
                _canExecuteTranspose = value;
                TransposeButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }
        public bool OnCanExecuteTranspose(object obj)
        {
            return CanExecuteTranspose;
        }
        public DelegateCommand<object> TransposeButtonClickedCommand { get; private set; }

        private List<TranspositionData> _transData = new List<TranspositionData>();
        public List<TranspositionData> TransData
        {
            get { return _transData; }
            set
            {
                _transData = value;
                OnPropertyChanged(() => TransData);
            }
        }

        private void Transpose()
        {
            Infrastructure.Dimensions.Keys.Key = SelectedKey;
            _transpositionLog = new List<TranspositionData>();
            TranspositionData data = null;
            RawInterval = Interval;
            if (Mode != _Enum.TranspositionMode.Octave) Direction = (RawInterval > 0) ? _Enum.Direction.Up : _Enum.Direction.Down;
            Interval = (RawInterval < 0) ? RawInterval + 12 : RawInterval;
            var selectedNotes = new ObservableCollection<Repository.DataService.Note>(Infrastructure.Support.Selection.Notes.OrderBy(p => p.StartTime));
            string restoreTargetAccidental = EditorState.TargetAccidental;
            int cnt = 0;
            foreach (Repository.DataService.Note note in selectedNotes)
            {
                cnt++;
                try
                {
                    if (NoteController.IsRest(note)) continue;
                    _before = new TranspositionState(note);
                    if (Mode == _Enum.TranspositionMode.Octave)
                    {
                        EditorState.TargetAccidental = _before.Accidental;
                    }
                    _after = new TranspositionState(_before, RawInterval, DeltaOctave, Mode, Interval);
                    data = AddTranspositionRecord(note, RawInterval, CurrentKey.Name.ToString(CultureInfo.InvariantCulture));
                    _after.Key = SelectedKey;
                    EA.GetEvent<CommitTransposition>().Publish(new Tuple<Guid, object>(note.Id, _after));
                    SetValidationIndicators(_before, _after);
                    data = data.AddTranspositionRecord(data, Direction.ToString(), note, _locationValidation, _octaveValidation, _slotValidation, _after.DeltaSlot);
                    _transpositionLog.Add(data);
                }
                catch (Exception ex)
                {
                    Exceptions.HandleException(data != null
                                                   ? string.Format("{0} {1} This note was not transposed. {2}",
                                                                   note.Pitch, note.StartTime, ex.Message)
                                                   : string.Format("{0} {1}", cnt, ex.Message));
                }
            }
            EditorState.TargetAccidental = restoreTargetAccidental;
            TransData = _transpositionLog;
            _transpositionLog = null;
        }

        private TranspositionData AddTranspositionRecord(Repository.DataService.Note note, int interval, string currentKey)
        {
            TranspositionData data;
            if (_transpositionLog.Count == 0)
            {
                data = new TranspositionData();
                data = data.Initialize();
                _transpositionLog.Add(data);
            }
            data = new TranspositionData();
            data = data.AddTranspositionRecord(data, interval.ToString(CultureInfo.InvariantCulture), currentKey, SelectedKey.Name, note);
            return data;
        }

        private void SetValidationIndicators(TranspositionState before, TranspositionState after)
        {
            _locationValidation = (Direction == _Enum.Direction.Up && after.Location_Y > before.Location_Y ||
                             Direction == _Enum.Direction.Down && after.Location_Y < before.Location_Y) ? "**" : "";

            _octaveValidation = (Direction == _Enum.Direction.Up && after.Octave < before.Octave ||
                                              Direction == _Enum.Direction.Down && after.Octave > before.Octave ||
                                              Mode == _Enum.TranspositionMode.Octave && before.Octave == after.Octave) ? "**" : "";

            _slotValidation = (Direction == _Enum.Direction.Up && int.Parse(before.Slot) > int.Parse(after.Slot) ||
                                            Direction == _Enum.Direction.Down && int.Parse(before.Slot) < int.Parse(after.Slot)) ? "**" : "";
        }

        public void OnTransposeButtonClicked(object obj)
        {
            //transposition process transposes only selected notes. If no notes are selected, then default to the entire composition.
            if (!Infrastructure.Support.Selection.Notes.Any())
            {
                EA.GetEvent<SelectComposition>().Publish(string.Empty);
            }
            switch (Mode)
            {
                case _Enum.TranspositionMode.Key:
                    Interval = SelectedKey.Index - CurrentKey.Index;

                    Transpose();
                    CurrentKey = SelectedKey;
                    break;
                case _Enum.TranspositionMode.Interval:
                    Transpose();
                    break;
                case _Enum.TranspositionMode.Octave:
                    Interval = 0;
                    Transpose();
                    break;
            }
            foreach (Repository.DataService.Measure measure in Infrastructure.Support.Selection.ImpactedMeasures)
            {
                EA.GetEvent<UpdateSpanManager>().Publish(measure.Id);
                EA.GetEvent<SpanMeasure>().Publish(measure);
            }
        }

        #endregion

        #region Cancel Button Support

        private bool _canExecuteCancel;
        public bool CanExecuteCancel
        {
            get { return _canExecuteCancel; }
            set
            {
                _canExecuteCancel = value;
                CancelButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }
        public bool OnCanExecuteCancel(object obj)
        {
            return CanExecuteCancel;
        }
        public DelegateCommand<object> CancelButtonClickedCommand { get; private set; }

        public void OnCancelButtonClicked(object obj)
        {
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
            EA.GetEvent<HideTransposePanel>().Publish(string.Empty);
        }

        #endregion

        #region Transpose CheckBox Support

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeIntervalCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeIntervalCheckedCommand
        {
            get { return _transposeIntervalCheckedCommand; }
            set
            {
                _transposeIntervalCheckedCommand = value;
                OnPropertyChanged(() => TransposeIntervalCheckedCommand);
            }
        }

        public void OnTransposeIntervalCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            IntervalTranspositionEnabled = true;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeIntervalUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeIntervalUncheckedCommand
        {
            get { return _transposeIntervalUncheckedCommand; }
            set
            {
                _transposeIntervalUncheckedCommand = value;
                OnPropertyChanged(() => TransposeIntervalUncheckedCommand);
            }
        }

        public void OnTransposeIntervalUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            IntervalTranspositionEnabled = false;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeOctaveCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeOctaveCheckedCommand
        {
            get { return _transposeOctaveCheckedCommand; }
            set
            {
                _transposeOctaveCheckedCommand = value;
                OnPropertyChanged(() => TransposeOctaveCheckedCommand);
            }
        }

        public void OnTransposeOctaveCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            OctaveTranspositionEnabled = true;
            DeltaOctave = 0;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeOctaveUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeOctaveUncheckedCommand
        {
            get { return _transposeOctaveUncheckedCommand; }
            set
            {
                _transposeOctaveUncheckedCommand = value;
                OnPropertyChanged(() => TransposeOctaveUncheckedCommand);
            }
        }

        public void OnTransposeOctaveUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            OctaveTranspositionEnabled = false;
            DeltaOctave = 0;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeKeyCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeKeyCheckedCommand
        {
            get { return _transposeKeyCheckedCommand; }
            set
            {
                _transposeKeyCheckedCommand = value;
                OnPropertyChanged(() => TransposeKeyCheckedCommand);
            }
        }

        public void OnTransposeKeyCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            KeyTranspositionEnabled = true;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _transposeKeyUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> TransposeKeyUncheckedCommand
        {
            get { return _transposeKeyUncheckedCommand; }
            set
            {
                _transposeKeyUncheckedCommand = value;
                OnPropertyChanged(() => TransposeKeyUncheckedCommand);
            }
        }

        public void OnTransposeKeyUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            KeyTranspositionEnabled = false;
        }

        #endregion

        #region Interval Transpose Radio Button Support

        private ExtendedDelegateCommand<ExtendedCommandParameter> _intervalUpUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> IntervalUpUncheckedCommand
        {
            get { return _intervalUpUncheckedCommand; }
            set
            {
                _intervalUpUncheckedCommand = value;
                OnPropertyChanged(() => IntervalUpUncheckedCommand);
            }
        }

        public void OnIntervalUpUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.None;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _intervalUpCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> IntervalUpCheckedCommand
        {
            get { return _intervalUpCheckedCommand; }
            set
            {
                _intervalUpCheckedCommand = value;
                OnPropertyChanged(() => IntervalUpCheckedCommand);
            }
        }

        public void OnIntervalUpCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.Up;
            Interval = Math.Abs(SelectedInterval.Value);
            CanExecuteTranspose = IsTransposable();
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _intervalDownUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> IntervalDownUncheckedCommand
        {
            get { return _intervalDownUncheckedCommand; }
            set
            {
                _intervalDownUncheckedCommand = value;
                OnPropertyChanged(() => IntervalDownUncheckedCommand);
            }
        }

        public void OnIntervalDownUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.None;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _intervalDownCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> IntervalDownCheckedCommand
        {
            get { return _intervalDownCheckedCommand; }
            set
            {
                _intervalDownCheckedCommand = value;
                OnPropertyChanged(() => IntervalDownCheckedCommand);
            }
        }

        public void OnIntervalDownCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.Down;
            Interval = -Math.Abs(SelectedInterval.Value);
            CanExecuteTranspose = IsTransposable();
        }

        #endregion

        #region Octave Transpose Radio Button Support

        private ExtendedDelegateCommand<ExtendedCommandParameter> _octaveUpCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> OctaveUpCheckedCommand
        {
            get { return _octaveUpCheckedCommand; }
            set
            {
                _octaveUpCheckedCommand = value;
                OnPropertyChanged(() => OctaveUpCheckedCommand);
            }
        }

        public void OnOctaveUpCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.Up;
            Interval = (Direction == _Enum.Direction.Up) ? int.Parse((1 * 12).ToString(CultureInfo.InvariantCulture)) : int.Parse((-1 * 12).ToString(CultureInfo.InvariantCulture));
            DeltaOctave = int.Parse(1.ToString(CultureInfo.InvariantCulture));
            CanExecuteTranspose = IsTransposable();
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _octaveUpUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> OctaveUpUncheckedCommand
        {
            get { return _octaveUpUncheckedCommand; }
            set
            {
                _octaveUpUncheckedCommand = value;
                OnPropertyChanged(() => OctaveUpUncheckedCommand);
            }
        }

        public void OnOctaveUpUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.None;
            DeltaOctave = 0;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _octaveDownCheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> OctaveDownCheckedCommand
        {
            get { return _octaveDownCheckedCommand; }
            set
            {
                _octaveDownCheckedCommand = value;
                OnPropertyChanged(() => OctaveDownCheckedCommand);
            }
        }

        public void OnOctaveDownCheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.Down;
            Interval = (Direction == _Enum.Direction.Up) ? int.Parse((1 * 12).ToString(CultureInfo.InvariantCulture)) : int.Parse((-1 * 12).ToString(CultureInfo.InvariantCulture));
            DeltaOctave = -int.Parse(1.ToString(CultureInfo.InvariantCulture));
            CanExecuteTranspose = IsTransposable();
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _octaveDownUncheckedCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> OctaveDownUncheckedCommand
        {
            get { return _octaveDownUncheckedCommand; }
            set
            {
                _octaveDownUncheckedCommand = value;
                Direction = _Enum.Direction.None;
                OnPropertyChanged(() => OctaveDownUncheckedCommand);
            }
        }

        public void OnOctaveDownUncheckedCommand(ExtendedCommandParameter commandParameter)
        {
            Direction = _Enum.Direction.None;
            DeltaOctave = 0;
        }

        #endregion
    }

}
