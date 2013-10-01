using System;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Repository.DataService;
using Microsoft.Practices.Composite.Presentation.Commands;
using Composer.Infrastructure.Events;
using System.Collections.Generic;
using Composer.Infrastructure.Dimensions;
using System.Data.Services.Client;
using Microsoft.Practices.ServiceLocation;
using Composer.Repository;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class NewCompositionPanelViewModel : BaseViewModel, INewCompositionPanelViewModel
    {
        private _Enum.StaffConfiguration _staffConfiguration = Preferences.DefaultStaffConfiguration;

        private List<short> _clefIds;

        private static DataServiceRepository<Repository.DataService.Composition> _repository;
        private Repository.DataService.Composition _composition;

        public Repository.DataService.Composition Composition
        {
            get { return _composition; }
            set
            {
                if (value != _composition)
                {
                    _composition = value;
                    if (_composition != null)
                    {
                        Update();
                    }
                    OnPropertyChanged(() => Composition);
                }
            }
        }

        private string _titleValidatorText = "";
        public string TitleValidatorText
        {
            get { return _titleValidatorText; }
            set
            {
                if (value != _titleValidatorText)
                {
                    _titleValidatorText = value;
                    OnPropertyChanged(() => TitleValidatorText);
                }
            }
        }

        private string _grandStaffConfigurationDimensionMargin = "";
        public string Staff2DimensionMargin
        {
            get { return _grandStaffConfigurationDimensionMargin; }
            set
            {
                if (value != _grandStaffConfigurationDimensionMargin)
                {
                    _grandStaffConfigurationDimensionMargin = value;
                    OnPropertyChanged(() => Staff2DimensionMargin);
                }
            }
        }

        private string _simpleStaffConfigurationDimensionMargin = "";
        public string Staff1DimensionMargin
        {
            get { return _simpleStaffConfigurationDimensionMargin; }
            set
            {
                if (value != _simpleStaffConfigurationDimensionMargin)
                {
                    _simpleStaffConfigurationDimensionMargin = value;
                    OnPropertyChanged(() => Staff1DimensionMargin);
                }
            }
        }

        private string _staffgroupMargin = "";
        public string StaffgroupMargin
        {
            get { return _staffgroupMargin; }
            set
            {
                if (value != _staffgroupMargin)
                {
                    _staffgroupMargin = value;
                    OnPropertyChanged(() => StaffgroupMargin);
                }
            }
        }

        private double _scale = 1;
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (Math.Abs(value - _scale) > 0)
                {
                    _scale = value;
                    OnPropertyChanged(() => Scale);
                }
            }
        }

        private double _titleValidatorOpacity;
        public double TitleValidatorOpacity
        {
            get { return _titleValidatorOpacity; }
            set
            {
                if (Math.Abs(value - _titleValidatorOpacity) > 0)
                {
                    _titleValidatorOpacity = value;
                    OnPropertyChanged(() => TitleValidatorOpacity);
                }
            }
        }

        private Visibility _compositionPanelVisibility = Visibility.Collapsed;
        public Visibility CompositionPanelVisibility
        {
            get { return _compositionPanelVisibility; }
            set
            {
                if (value != _compositionPanelVisibility)
                {
                    _compositionPanelVisibility = value;
                    OnPropertyChanged(() => CompositionPanelVisibility);
                }
            }
        }

        private Visibility _titleValidatorVisibility = Visibility.Collapsed;
        public Visibility TitleValidatorVisibility
        {
            get { return _titleValidatorVisibility; }
            set
            {
                if (value != _titleValidatorVisibility)
                {
                    _titleValidatorVisibility = value;
                    OnPropertyChanged(() => TitleValidatorVisibility);
                }
            }
        }

        private Visibility _grandStaffConfigurationClefComboBoxVisibility = Visibility.Collapsed;
        public Visibility GrandStaffConfigurationClefComboBoxVisibility
        {
            get { return _grandStaffConfigurationClefComboBoxVisibility; }
            set
            {
                if (value != _grandStaffConfigurationClefComboBoxVisibility)
                {
                    _grandStaffConfigurationClefComboBoxVisibility = value;
                    OnPropertyChanged(() => GrandStaffConfigurationClefComboBoxVisibility);
                }
            }
        }

        private short GetClefId(Staff staff)
        {
            short clefId = (from a in Infrastructure.Dimensions.Clefs.ClefList where a.Id == SelectedSimpleStaffConfigurationClef.Id select a).Single().Id;
            if (staff.Sequence == Defaults.SequenceIncrement)
            {
                clefId = (from a in Infrastructure.Dimensions.Clefs.ClefList where a.Id == SelectedGrandStaffConfigurationClef.Id select a).Single().Id;
            }
            return clefId;
        }

        private void Update()
        {
            //for simplicity, we are updating all dimensions each time a single dimension changes.
            if (Composition == null) return;
            UpdateInfo(); //update left information panel
            _clefIds = new List<short>();
            Infrastructure.Dimensions.Keys.Key = SelectedKey;
            var keyId = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Id == SelectedKey.Id select a).Single().Id;
            EA.GetEvent<UpdateStaffDimensionWidth>().Publish(keyId);
            var timeSignatureId = (from a in Infrastructure.Dimensions.TimeSignatures.TimeSignatureList where a.Id == SelectedTimeSignature.Id select a).Single().Id;
            var instrumentId = (from a in Infrastructure.Dimensions.Instruments.InstrumentList where a.Id == SelectedInstrument.Id select a).Single().Id;

            Composition.Instrument_Id = instrumentId;
            Composition.Key_Id = keyId;
            Composition.TimeSignature_Id = timeSignatureId;
            foreach (var staffgroup in Composition.Staffgroups)
            {
                staffgroup.Key_Id = keyId;
                foreach (var staff in staffgroup.Staffs)
                {
                    staff.Clef_Id = GetClefId(staff);
                    _clefIds.Add((short)staff.Clef_Id);
                    staff.Key_Id = keyId;
                    staff.TimeSignature_Id = timeSignatureId;
                    foreach (var measure in staff.Measures)
                    {
                        measure.Key_Id = keyId;
                        measure.TimeSignature_Id = timeSignatureId;
                    }
                }
            }
        }

        private void UpdateInfo()
        {
            TimeSignatureCaption = string.Format("Time Signature: {0}", SelectedTimeSignature.Name.Replace(',', '/'));
            TimeSignatureDescription = SelectedTimeSignature.Description;
            KeyCaption = string.Format("Key: {0}", SelectedKey.Name);
        }

        private string _timeSignatureCaption = string.Empty;
        public string TimeSignatureCaption
        {
            get { return _timeSignatureCaption; }
            set
            {
                _timeSignatureCaption = value;
                OnPropertyChanged(() => TimeSignatureCaption);
            }
        }

        private string _timeSignatureDescription = string.Empty;
        public string TimeSignatureDescription
        {
            get { return _timeSignatureDescription; }
            set
            {
                _timeSignatureDescription = value;
                OnPropertyChanged(() => TimeSignatureDescription);
            }
        }

        private string _keyCaption = string.Empty;
        public string KeyCaption
        {
            get { return _keyCaption; }
            set
            {
                _keyCaption = value;
                OnPropertyChanged(() => KeyCaption);
            }
        }

        private int _height = 300;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged(() => Height);
            }
        }

        private static Int16 _measureIndex;
        private static int _staffgroupSequence;
        private static int _staffSequence;
        private static int _measureSequence;

        public void CreateNewCompositionForPanel()
        {
            _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            var staffgroups = new DataServiceCollection<Staffgroup>(null, TrackingMode.None);

            int staffDensity = ((short)_staffConfiguration == (short)_Enum.StaffConfiguration.Grand) ?
                Infrastructure.Constants.Defaults.NewCompositionPanelGrandStaffConfigurationStaffDensity : Infrastructure.Constants.Defaults.NewCompositionPanelSimpleStaffConfigurationStaffDensity;

            Repository.DataService.Composition composition = CompositionManager.Create();
            for (int n = 0; n < Infrastructure.Constants.Defaults.NewCompositionPanelStaffgroupDensity; n++)
            {
                var staffgroup = StaffgroupManager.Create(composition.Id, _staffgroupSequence);
                _staffgroupSequence += Defaults.SequenceIncrement;
                var staffs = new DataServiceCollection<Staff>(null, TrackingMode.None);
                _staffSequence = 0;
                for (var j = 0; j < staffDensity; j++)
                {
                    var staff = StaffManager.Create(staffgroup.Id, _staffSequence);
                    _staffSequence += Defaults.SequenceIncrement;

                    //set clef to default grand staff clef, so the next time through this loop, the grand staff gets the correct clef.
                    Preferences.DefaultClefId = 0;

                    var measures = new DataServiceCollection<Repository.DataService.Measure>(null, TrackingMode.None);
                    _measureSequence = 0;
                    for (var k = 0; k < Infrastructure.Constants.Defaults.NewCompositionPanelMeasureDensity; k++)
                    {
                        var measure = MeasureManager.Create(staff.Id, _measureSequence);

                        _measureSequence += Defaults.SequenceIncrement;
                        measure.Index = _measureIndex;
                        _measureIndex++;
                        _repository.Context.AddLink(staff, "Measures", measure);
                        measures.Add(measure);
                    }
                    staff.Measures = measures;
                    _repository.Context.AddLink(staffgroup, "Staffs", staff);
                    staffs.Add(staff);
                }
                staffgroup.Staffs = staffs;
                _repository.Context.AddLink(composition, "Staffgroups", staffgroup);
                staffgroups.Add(staffgroup);
            }
            composition.Staffgroups = staffgroups;
            composition = CompositionManager.Flatten(composition);
            Composition = composition;
        }

        public NewCompositionPanelViewModel()
        {
            EditorState.IsNewCompositionPanel = true;

            //we are reusing vectors and such, but the newcompositionpanel, needs different vector widths for some vectors, so....
            //these values are set back normal in the start and cancel button handlers below.
            Preferences.MeasureWidth = Preferences.NewComppositionPanelMeasureWidth;
            Preferences.StaffDimensionAreaWidth = Preferences.NewComppositionPanelStaffDimensionAreaWidth;
            //

            #region Set DropDowns to Default Values
            Infrastructure.Dimensions.Keys.InitializeKeys();
            Keys = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Listable select a).ToList();
            SelectedKey = (from a in Infrastructure.Dimensions.Keys.KeyList where a.Name == Preferences.DefaultKey select a).Single();
            _selectedInstrument = (from a in Infrastructure.Dimensions.Instruments.InstrumentList where a.Name == Preferences.DefaultInstrument select a).Single();
            SelectedSimpleStaffConfigurationClef = (from a in Infrastructure.Dimensions.Clefs.ClefList where a.Id == Preferences.DefaultClefId select a).Single();
            SelectedGrandStaffConfigurationClef = (from a in Infrastructure.Dimensions.Clefs.ClefList where a.Id == Preferences.DefaultGrandStaffClefId select a).Single();
            SelectedTimeSignature = (from a in Infrastructure.Dimensions.TimeSignatures.TimeSignatureList where a.Id == Preferences.DefaultTimeSignatureId select a).Single();

            #endregion Set DropDowns to Default Values

            CreateNewCompositionForPanel();

            DefineCommands();
            SubscribeEvents();
            EA.GetEvent<SetProvenancePanel>().Publish(string.Empty);
            Scale = 1;
            Update();
            EA.GetEvent<SetNewCompositionTitleForeground>().Publish("#CCCCCC");
            SetMargins();
            GrandStaffConfigurationClefComboBoxVisibility = (_staffConfiguration == _Enum.StaffConfiguration.Grand) ? Visibility.Visible : Visibility.Collapsed;
            EditorState.StaffConfiguration = _staffConfiguration;
            CompositionPanelVisibility = Visibility.Visible;
        }

        private void SetMargins()
        {
            switch ((int)Composition.StaffConfiguration)
            {
                case (int)_Enum.StaffConfiguration.Simple:
                    Staff1DimensionMargin = Finetune.NewCompositionPanel.staff1ComboBoxMarginForSimpleStaffConfiguration;
                    StaffgroupMargin = Finetune.NewCompositionPanel.staffgroupMarginSimpleStaffConfiguration;
                    break;
                case (int)_Enum.StaffConfiguration.Grand:
                    Staff1DimensionMargin = Finetune.NewCompositionPanel.staff1ComboBoxMarginForGrandStaffConfiguration;
                    Staff2DimensionMargin = Finetune.NewCompositionPanel.staff2ComboBoxMarginForGrandStaffConfiguration;
                    StaffgroupMargin = Finetune.NewCompositionPanel.staffgroupMarginGrandStaffConfiguration;
                    break;
                default :
                    //same as 'simple'
                    Staff1DimensionMargin = Finetune.NewCompositionPanel.staff1ComboBoxMarginForSimpleStaffConfiguration;
                    StaffgroupMargin = Finetune.NewCompositionPanel.staffgroupMarginSimpleStaffConfiguration;
                    break;
            }
        }

        private string _titleForeground = "#cccccc";
        public string TitleForeground
        {
            get { return _titleForeground; }
            set
            {
                if (value != _titleForeground)
                {
                    _titleForeground = value;
                    OnPropertyChanged(() => TitleForeground);
                }
            }
        }

        private string _title = Infrastructure.Constants.Messages.NewCompositionPanelTitlePrompt;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChanged(() => Title);
                }
            }
        }

        #region Dimensions

        private List<Key> _keys = Infrastructure.Dimensions.Keys.KeyList;
        public List<Key> Keys
        {
            get 
            { 
                return _keys; 
            }
            set
            {
                _keys = value;
                OnPropertyChanged(() => Keys);
            }
        }

        private Key _selectedKey;
        public Key SelectedKey
        {
            get { return _selectedKey; }
            set
            {
                if (value != _selectedKey)
                {
                    _selectedKey = value;
                    Update();
                    OnPropertyChanged(() => SelectedKey);
                }
            }
        }

        public DelegateCommand<Key> KeySelectedCommand { get; private set; }

        private List<Instrument> _instruments = Infrastructure.Dimensions.Instruments.InstrumentList;
        public List<Instrument> Instruments
        {
            get { return _instruments; }
            set
            {
                _instruments = value;
                OnPropertyChanged(() => _instruments);
            }
        }

        private Instrument _selectedInstrument;
        public Instrument SelectedInstrument
        {
            get { return _selectedInstrument; }
            set
            {
                if (value != _selectedInstrument)
                {
                    _selectedInstrument = value;
                    Update();
                    OnPropertyChanged(() => SelectedInstrument);
                }
            }
        }

        public DelegateCommand<Instrument> InstrumentSelectedCommand { get; private set; }

        private List<Clef> _clefs = Infrastructure.Dimensions.Clefs.ClefList;
        public List<Clef> Clefs
        {
            get { return _clefs; }
            set
            {
                _clefs = value;
                OnPropertyChanged(() => Clefs);
            }
        }

        private Clef _selectedGrandStaffConfigurationClef;
        public Clef SelectedGrandStaffConfigurationClef
        {
            get { return _selectedGrandStaffConfigurationClef; }
            set
            {
                if (value != _selectedGrandStaffConfigurationClef)
                {
                    _selectedGrandStaffConfigurationClef = value;
                    Update();
                    OnPropertyChanged(() => SelectedGrandStaffConfigurationClef);
                }
            }
        }

        private Clef _selectedSimpleStaffConfigurationClef;
        public Clef SelectedSimpleStaffConfigurationClef
        {
            get { return _selectedSimpleStaffConfigurationClef; }
            set
            {
                if (value != _selectedSimpleStaffConfigurationClef)
                {
                    _selectedSimpleStaffConfigurationClef = value;
                    Update();
                    OnPropertyChanged(() => SelectedSimpleStaffConfigurationClef);
                }
            }
        }

        private List<TimeSignature> _timeSignatures = Infrastructure.Dimensions.TimeSignatures.TimeSignatureList;
        public List<TimeSignature> TimeSignatures
        {
            get { return _timeSignatures; }
            set
            {
                _timeSignatures = value;
                OnPropertyChanged(() => TimeSignatures);
            }
        }

        public DelegateCommand<TimeSignature> TimeSignatureSelectedCommand { get; private set; }

        private TimeSignature _selectedTimeSignature;
        public TimeSignature SelectedTimeSignature
        {
            get { return _selectedTimeSignature; }
            set
            {
                if (value != _selectedTimeSignature)
                {
                    _selectedTimeSignature = value;
                    Update();
                    OnPropertyChanged(() => SelectedTimeSignature);
                }
            }
        }

        #endregion

        public void OnShowNewCompositionTitleValidator(string message)
        {
            TitleValidatorText = message;
            TitleValidatorOpacity = 1;
        }

        public void OnHideNewCompositionTitleValidator(object obj)
        {
            TitleValidatorOpacity = 0;
        }

        public void OnSetNewCompositionTitleForeground(string color)
        {
            TitleForeground = color;
        }

        public void OnNewCompositionPanelStaffConfigurationChanged(_Enum.StaffConfiguration staffConfiguration)
        {
            CompositionPanelVisibility = Visibility.Collapsed;
            EditorState.StaffConfiguration = staffConfiguration;
            _staffConfiguration = staffConfiguration;
            DetachNewCompositionPanelComposition();
            Composition = null;

            CreateNewCompositionForPanel();
            if (Composition == null)
            {
                throw new ArgumentNullException("NewCompositionViewModel.OnNewCompositionPanelStaffConfigurationChanged.Composition");
            }
            Composition.StaffConfiguration = (short)staffConfiguration;
            SetMargins();
            GrandStaffConfigurationClefComboBoxVisibility = (staffConfiguration == _Enum.StaffConfiguration.Grand) ? Visibility.Visible : Visibility.Collapsed;
            CompositionPanelVisibility = Visibility.Visible;
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<ShowNewCompositionTitleValidator>().Subscribe(OnShowNewCompositionTitleValidator);
            EA.GetEvent<HideNewCompositionTitleValidator>().Subscribe(OnHideNewCompositionTitleValidator);
            EA.GetEvent<SetNewCompositionTitleForeground>().Subscribe(OnSetNewCompositionTitleForeground);
            EA.GetEvent<NewCompositionPanelStaffConfigurationChanged>().Subscribe(OnNewCompositionPanelStaffConfigurationChanged);
        }

        private void ResetDimensions()
        {
            Infrastructure.Support.Densities.StaffgroupDensity = Infrastructure.Constants.Defaults.DefaultStaffgroupDensity;

            Infrastructure.Support.Densities.StaffDensity = (_staffConfiguration == _Enum.StaffConfiguration.Grand) ?
                Infrastructure.Constants.Defaults.DefaultGrandStaffStaffDensity : Infrastructure.Constants.Defaults.DefaultSimpleStaffStaffDensity;

            Infrastructure.Support.Densities.MeasureDensity = Infrastructure.Constants.Defaults.DefaultMeasureDensity;
        }

        private void DetachNewCompositionPanelComposition()
        {
            if (Composition != null)
            {
                for (var i = 0; i < Composition.Staffgroups.Count; i++)
                {
                    var staffgroup = Composition.Staffgroups[i];
                    for (var j = 0; j < staffgroup.Staffs.Count; j++)
                    {
                        var staff = _composition.Staffgroups[i].Staffs[j];
                        foreach (var measure in staff.Measures)
                        {
                            _repository.Context.Detach(measure);
                        }
                        _repository.Context.Detach(staff);
                    }
                    _repository.Context.Detach(staffgroup);
                }
                _repository.Context.Detach(_composition);
            }
        }

        private void DefineCommands()
        {
            StartButtonClickedCommand = new DelegateCommand<object>(OnStartButtonClicked, OnCanExecuteStart);
            CancelButtonClickedCommand = new DelegateCommand<object>(OnCancelButtonClicked, OnCanExecuteCancel);

            CanExecuteStart = true;
            CanExecuteCancel = true;

            KeySelectedCommand = new DelegateCommand<Key>(OnKeySelectedCommand);
            InstrumentSelectedCommand = new DelegateCommand<Instrument>(OnInstrumentSelectedCommand);
            TimeSignatureSelectedCommand = new DelegateCommand<TimeSignature>(OnTimeSignatureSelectedCommand);
        }

        #region Event handlers

        private void OnKeySelectedCommand(Key key)
        {
            Infrastructure.Dimensions.Keys.Key = key;
        }

        private void OnInstrumentSelectedCommand(Instrument instrument)
        {
            Infrastructure.Dimensions.Instruments.Instrument = instrument;
        }

        private void OnTimeSignatureSelectedCommand(TimeSignature timeSignature)
        {
            Infrastructure.Dimensions.TimeSignatures.TimeSignature = timeSignature;
        }

        #endregion

        #region Start Button Support

        private bool _canExecuteStart = true;
        public bool CanExecuteStart
        {
            get { return _canExecuteStart; }
            set
            {
                _canExecuteStart = value;
                StartButtonClickedCommand.RaiseCanExecuteChanged();
            }
        }

        public bool OnCanExecuteStart(object obj)
        {
            return CanExecuteStart;
        }

        public DelegateCommand<object> StartButtonClickedCommand { get; private set; }

        public void OnStartButtonClicked(object obj)
        {
            Infrastructure.Dimensions.Keys.Key = SelectedKey;
            if (Title.IndexOf(Infrastructure.Constants.Messages.NewCompositionPanelTitlePrompt, StringComparison.Ordinal) >= 0 || Title.Trim().Length == 0)
            {
                EA.GetEvent<ShowNewCompositionTitleValidator>().Publish(Infrastructure.Constants.Messages.NewCompositionPanelTitleValidationPrompt);
                return;
            }
            ResetDimensions();
            Preferences.DefaultClefId = 1;
            EditorState.IsNewCompositionPanel = false;
            Preferences.MeasureWidth = Preferences.CompositionMeasureWidth;
            Preferences.StaffDimensionAreaWidth = Preferences.CompositionStaffDimensionAreaWidth;

            EA.GetEvent<HideNewCompositionPanel>().Publish(string.Empty);

            var authorIds = new List<string> {Current.User.Id};
            var payload = new Tuple<string, List<string>, _Enum.StaffConfiguration, List<short>>(Title, authorIds, _staffConfiguration, _clefIds);
            DetachNewCompositionPanelComposition();
            EA.GetEvent<CreateNewComposition>().Publish(payload);
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
            Composition = null;
            EditorState.IsNewCompositionPanel = false;

            Preferences.MeasureWidth = Preferences.CompositionMeasureWidth;
            Preferences.StaffDimensionAreaWidth = Preferences.CompositionStaffDimensionAreaWidth;
            EA.GetEvent<HideNewCompositionPanel>().Publish(string.Empty);
            OnClick(null);
            EA.GetEvent<ShowHub>().Publish(string.Empty);
        }

        #endregion
    }
}
