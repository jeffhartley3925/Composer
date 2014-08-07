using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Behavior;
using System.Windows.Input;
using System.Windows;
using Composer.Repository.DataService;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using System.Collections.Generic;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Dimensions;
using System.Data.Services.Client;
using Composer.Repository;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class StaffViewModel : BaseViewModel, IStaffViewModel, IEventCatcher
    {
        DataServiceRepository<Repository.DataService.Composition> _repository;

        private Visibility _clefVisibility = Visibility.Visible;
        public Visibility ClefVisibility
        {
            get { return _clefVisibility; }
            set
            {
                _clefVisibility = value;
                OnPropertyChanged(() => ClefVisibility);
            }
        }

        private Visibility _keyVisibility = Visibility.Visible;
        public Visibility KeyVisibility
        {
            get { return _keyVisibility; }
            set
            {
                _keyVisibility = value;
                OnPropertyChanged(() => KeyVisibility);
            }
        }

        private Visibility _timeSignatureVisibility = Visibility.Visible;
        public Visibility TimeSignatureVisibility
        {
            get { return _timeSignatureVisibility; }
            set
            {
                _timeSignatureVisibility = value;
                OnPropertyChanged(() => TimeSignatureVisibility);
            }
        }

        private double _dimensionAddjustmentX = 0;
        public double DimensionAdjustment_X
        {
            get { return _dimensionAddjustmentX; }
            set
            {
                _dimensionAddjustmentX = value;
                OnPropertyChanged(() => DimensionAdjustment_X);
            }
        }

        private string _staffLinesMargin = "0,7,0,7";
        public string StaffLinesMargin
        {
            get { return _staffLinesMargin; }
            set
            {
                _staffLinesMargin = value;
                OnPropertyChanged(() => StaffLinesMargin);
            }
        }

        private string _barVector = string.Format(Bars.StaffBarVectorFormatter, Defaults.staffLinesHeight);
        public string BarVector
        {
            get { return _barVector; }
            set
            {
                _barVector = value;
                OnPropertyChanged(() => BarVector);
            }
        }

        private string _barMargin = "0,0,0,0";
        public string BarMargin
        {
            get { return _barMargin; }
            set
            {
                _barMargin = value;
                OnPropertyChanged(() => BarMargin);
            }
        }

        private int _vectorId = 24;
        public int Vector_Id
        {
            get { return _vectorId; }
            set
            {
                _vectorId = value;
                OnPropertyChanged(() => Vector_Id);
            }
        }

        public StaffViewModel(string id, string sGId, string index)
        {
            Staff = Utils.GetStaff(Guid.Parse(id));
            Width = Preferences.MeasureWidth * Defaults.DefaultMeasureDensity;

            SetPropertiesForNewCompositionPanelDimensionControls();
            EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);

            DefineCommands();
            SubscribeEvents();
        }

        private void SetPropertiesForNewCompositionPanelDimensionControls()
        {
            if (EditorState.IsNewCompositionPanel)
            {
                StaffLinesMargin = "0,8,0,7";
                DimensionAdjustment_X = 58;
                ClefVisibility = Visibility.Collapsed;
                KeyVisibility = (Staff.Sequence == 0) ? Visibility.Collapsed : Visibility.Visible;
                TimeSignatureVisibility = (Staff.Sequence == 0) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Repository.DataService.Measure _selectedMeasure;
        public Repository.DataService.Measure SelectedMeasure
        {
            get { return _selectedMeasure; }
            set
            {
                _selectedMeasure = value;
                OnPropertyChanged(() => SelectedMeasure);
            }
        }

        private Staff _staff;
        public Repository.DataService.Staff Staff
        {
            get { return _staff; }
            set
            {
                _staff = value;
                if (_staff.Clef_Id != null) Clef_Id = (int)_staff.Clef_Id;
                OnPropertyChanged(() => Staff);
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
            }
        }

        private int _keyId;
        public int Key_Id
        {
            get { return _keyId; }
            set
            {
                _keyId = value;
                OnPropertyChanged(() => Key_Id);
            }
        }

        private int _clefId;
        public int Clef_Id
        {
            get { return _clefId; }
            set
            {
                _clefId = value;
                OnPropertyChanged(() => Clef_Id);
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private int _flatVectorId = Preferences.FlatVectorId;
        public int FlatVector_Id
        {
            get { return _flatVectorId; }
            set
            {
                _flatVectorId = value;
                OnPropertyChanged(() => FlatVector_Id);
            }
        }

        private int _sharpVectorId = Preferences.SharpVectorId;
        public int SharpVector_Id
        {
            get { return _sharpVectorId; }
            set
            {
                _sharpVectorId = value;
                OnPropertyChanged(() => SharpVector_Id);
            }
        }

        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            if (commandParameter.EventArgs.GetType() == typeof(MouseEventArgs))
            {

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

        public void DefineCommands()
        {
            MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, null);
            MouseLeftButtonUpCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonUpCommand, null);
        }

        public void OnMouseLeftButtonUpCommand(ExtendedCommandParameter commandParameter)
        {
            EA.GetEvent<DeSelectAll>().Publish(string.Empty);
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeftButtonUpCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpCommand
        {
            get
            {
                return _mouseLeftButtonUpCommand;
            }
            set
            {
                _mouseLeftButtonUpCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpCommand);
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

        public void SubscribeEvents()
        {
            EA.GetEvent<SendMeasureClickToStaff>().Subscribe(OnClick, true);
            EA.GetEvent<SelectStaff>().Subscribe(OnSelectStaff);
            EA.GetEvent<UpdateVerseIndexes>().Subscribe(OnUpdateVerseIndexes);
            EA.GetEvent<CommitTransposition>().Subscribe(OnCommitTransposition, true);
            EA.GetEvent<AddArc>().Subscribe(OnAddArc);
            EA.GetEvent<BroadcastArcs>().Subscribe(OnBroadcastArcs);
            EA.GetEvent<DeleteArc>().Subscribe(OnDeleteArc);
            EA.GetEvent<AdjustBracketHeight>().Subscribe(OnAdjustBracketHeight);
        }

        public void OnAdjustBracketHeight(object obj)
        {
            double barHeight = Defaults.staffLinesHeight;
            if (!EditorState.IsNewCompositionPanel)
            {
                var staffConfiguration = (_Enum.StaffConfiguration)CompositionManager.Composition.StaffConfiguration;
                switch (staffConfiguration)
                {
                    case _Enum.StaffConfiguration.Grand:
                        if (Staff.Sequence == 0)
                        {
                            barHeight = (Defaults.BracketHeightBaseline + (EditorState.VerseCount * Defaults.VerseHeight)) / 2;
                            BarMargin = "0,46,0,0";
                        }
                        else
                        {
                            barHeight = -(Defaults.BracketHeightBaseline + (EditorState.VerseCount * Defaults.VerseHeight)) / 2;
                            BarMargin = "0,78,0,0";
                        }
                        break;
                    case _Enum.StaffConfiguration.Simple:
                        barHeight = Defaults.staffLinesHeight;
                        BarMargin = "0,46,0,0";
                        break;
                    case _Enum.StaffConfiguration.MultiInstrument:
                        break;
                }
            }
            BarVector = string.Format(Bars.StaffBarVectorFormatter, barHeight);
        }

        public void OnBroadcastArcs(object obj)
        {
            var arcs = (DataServiceCollection<Arc>)obj;
            foreach (var a in arcs.Where(a => a.Staff_Id == Staff.Id).Where(a => !Staff.Arcs.Contains(a)))
            {
                Staff.Arcs.Add(a);
            }
        }

        private string _foreground = Preferences.StaffForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Background);
            }
        }

        private void SetRepository()
        {
            if (_repository == null)
            {
                _repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }

        public void OnDeleteArc(Guid arcId)
        {
            foreach (var arc in Staff.Arcs.Where(arc => arc.Id == arcId))
            {
                SetRepository();
                Staff.Arcs.Remove(arc);
                _repository.Delete(arc);
                break;
            }
        }

        public void OnAddArc(Arc arc)
        {
            if (arc.Staff_Id == Staff.Id)
            {
                if (!Staff.Arcs.Contains(arc))
                {
                    CompositionManager.Composition.Arcs.Add(arc);
                    Staff.Arcs.Add(arc);
                    Staff = Staff;
                }
            }
        }

        public void OnCommitTransposition(Tuple<Guid, object> payload)
        {
            var state = (TranspositionState)payload.Item2;
            Key_Id = state.Key.Id;
            Staff.Key_Id = state.Key.Id;
        }

        private List<int> _verseIndexes;
        public List<int> VerseIndexes
        {
            get { return _verseIndexes; }
            set
            {
                _verseIndexes = value;
                OnPropertyChanged(() => VerseIndexes);
            }
        }

        public void OnUpdateVerseIndexes(int verseCount)
        {
            VerseNumbersVisibility = Visibility.Collapsed;
            if (verseCount <= 0) return;
            //tmp will be null if this viewModel is one spun up for the NewCompositionPanel
            var sequence = Utils.GetStaffgroupSequenceById(Staff.Staffgroup_Id);
            // verse numbers only visible in first staff of first staffgroup.
            if (sequence != 0 || Staff.Sequence != 0) return;
            VerseNumbersVisibility = Visibility.Visible;
            VerseIndexes = new List<int>();
            for (var i = 1; i <= verseCount; i++)
            {
                VerseIndexes.Add(i);
            }
        }

        private Visibility _verseNumbersVisibility = Visibility.Collapsed;
        public Visibility VerseNumbersVisibility
        {
            get { return _verseNumbersVisibility; }
            set
            {
                _verseNumbersVisibility = value;
                OnPropertyChanged(() => VerseNumbersVisibility);
            }
        }

        public override void OnClick(object obj)
        {
            Guid guid;
            if (Guid.TryParse(obj.ToString(), out guid) && guid == Staff.Id)
            {
                EA.GetEvent<StaffClicked>().Publish(Staff.Staffgroup_Id);
            }
        }

        public void OnSelectStaff(Guid id)
        {
            if (Staff.Id == id)
            {
                foreach (var measure in Staff.Measures)
                {
                    EA.GetEvent<SelectMeasure>().Publish(measure.Id);
                }
            }
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}