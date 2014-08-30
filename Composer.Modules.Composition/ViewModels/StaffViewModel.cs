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
        DataServiceRepository<Repository.DataService.Composition> repository;

        private Visibility clefVisibility = Visibility.Visible;
        public Visibility ClefVisibility
        {
            get { return this.clefVisibility; }
            set
            {
                this.clefVisibility = value;
                OnPropertyChanged(() => ClefVisibility);
            }
        }

        private Visibility keyVisibility = Visibility.Visible;
        public Visibility KeyVisibility
        {
            get { return this.keyVisibility; }
            set
            {
                this.keyVisibility = value;
                OnPropertyChanged(() => KeyVisibility);
            }
        }

        private Visibility timeSignatureVisibility = Visibility.Visible;
        public Visibility TimeSignatureVisibility
        {
            get { return this.timeSignatureVisibility; }
            set
            {
                this.timeSignatureVisibility = value;
                OnPropertyChanged(() => TimeSignatureVisibility);
            }
        }

        private double dimensionAddjustmentX = 0;
        public double DimensionAdjustment_X
        {
            get { return this.dimensionAddjustmentX; }
            set
            {
                this.dimensionAddjustmentX = value;
                OnPropertyChanged(() => DimensionAdjustment_X);
            }
        }

        private string staffLinesMargin = "0,7,0,7";
        public string StaffLinesMargin
        {
            get { return this.staffLinesMargin; }
            set
            {
                this.staffLinesMargin = value;
                OnPropertyChanged(() => StaffLinesMargin);
            }
        }

        private string barVector = string.Format(Bars.StaffBarVectorFormatter, Defaults.staffLinesHeight);
        public string BarVector
        {
            get { return this.barVector; }
            set
            {
                this.barVector = value;
                OnPropertyChanged(() => BarVector);
            }
        }

        private string barMargin = "0,0,0,0";
        public string BarMargin
        {
            get { return this.barMargin; }
            set
            {
                this.barMargin = value;
                OnPropertyChanged(() => BarMargin);
            }
        }

        private int vectorId = 24;
        public int Vector_Id
        {
            get { return this.vectorId; }
            set
            {
                this.vectorId = value;
                OnPropertyChanged(() => Vector_Id);
            }
        }

        public StaffViewModel(string iD)
        {
            Staff = Utils.GetStaff(Guid.Parse(iD));
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

        private Repository.DataService.Measure selectedMeasure;
        public Repository.DataService.Measure SelectedMeasure
        {
            get { return this.selectedMeasure; }
            set
            {
                this.selectedMeasure = value;
                OnPropertyChanged(() => SelectedMeasure);
            }
        }

        private Staff staff;
        public Repository.DataService.Staff Staff
        {
            get { return this.staff; }
            set
            {
                this.staff = value;
                if (this.staff.Clef_Id != null) Clef_Id = (int)this.staff.Clef_Id;
                OnPropertyChanged(() => Staff);
            }
        }

        private int timeSignatureId;
        public int TimeSignature_Id
        {
            get { return this.timeSignatureId; }
            set
            {
                this.timeSignatureId = value;
                OnPropertyChanged(() => TimeSignature_Id);
            }
        }

        private int keyId;
        public int Key_Id
        {
            get { return this.keyId; }
            set
            {
                this.keyId = value;
                OnPropertyChanged(() => Key_Id);
            }
        }

        private int clefId;
        public int Clef_Id
        {
            get { return this.clefId; }
            set
            {
                this.clefId = value;
                OnPropertyChanged(() => Clef_Id);
            }
        }

        private int width;
        public int Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                OnPropertyChanged(() => Width);
            }
        }

        private int flatVectorId = Preferences.FlatVectorId;
        public int FlatVector_Id
        {
            get { return this.flatVectorId; }
            set
            {
                this.flatVectorId = value;
                OnPropertyChanged(() => FlatVector_Id);
            }
        }

        private int sharpVectorId = Preferences.SharpVectorId;
        public int SharpVector_Id
        {
            get { return this.sharpVectorId; }
            set
            {
                this.sharpVectorId = value;
                OnPropertyChanged(() => SharpVector_Id);
            }
        }

        public override void OnMouseMove(ExtendedCommandParameter commandParameter)
        {
            if (commandParameter.EventArgs.GetType() == typeof(MouseEventArgs))
            {

            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseMoveCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseMoveCommand
        {
            get { return this.mouseMoveCommand; }
            set
            {
                this.mouseMoveCommand = value;
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

        private ExtendedDelegateCommand<ExtendedCommandParameter> mouseLeftButtonUpCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeftButtonUpCommand
        {
            get
            {
                return this.mouseLeftButtonUpCommand;
            }
            set
            {
                this.mouseLeftButtonUpCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpCommand);
            }
        }

        private string background = Preferences.MeasureBackground;
        public string Background
        {
            get { return this.background; }
            set
            {
                this.background = value;
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
            var aRs = (DataServiceCollection<Arc>)obj;
            foreach (var a in aRs.Where(a => a.Staff_Id == Staff.Id).Where(a => !Staff.Arcs.Contains(a)))
            {
                Staff.Arcs.Add(a);
            }
        }

        private string foreground = Preferences.StaffForeground;
        public string Foreground
        {
            get { return this.foreground; }
            set
            {
                this.foreground = value;
                OnPropertyChanged(() => Background);
            }
        }

        private void SetRepository()
        {
            if (this.repository == null)
            {
                this.repository = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
            }
        }

        public void OnDeleteArc(Guid aRiD)
        {
            foreach (var arc in Staff.Arcs.Where(arc => arc.Id == aRiD))
            {
                SetRepository();
                Staff.Arcs.Remove(arc);
                this.repository.Delete(arc);
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

        private List<int> verseIndexes;
        public List<int> VerseIndexes
        {
            get { return this.verseIndexes; }
            set
            {
                this.verseIndexes = value;
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

        private Visibility verseNumbersVisibility = Visibility.Collapsed;
        public Visibility VerseNumbersVisibility
        {
            get { return this.verseNumbersVisibility; }
            set
            {
                this.verseNumbersVisibility = value;
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

        public void OnSelectStaff(Guid iD)
        {
            if (Staff.Id == iD)
            {
                foreach (var mE in Staff.Measures)
                {
                    EA.GetEvent<SelectMeasure>().Publish(mE.Id);
                }
            }
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}