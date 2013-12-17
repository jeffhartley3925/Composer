using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Behavior;
using System.Windows.Input;
using System.Windows;
using Composer.Modules.Composition.Views;
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
    public sealed class StaffViewModel : BaseViewModel, IStaffViewModel
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

        private string _saveButtonForeground = "#3b5998";
        public string SaveButtonForeground
        {
            get { return _saveButtonForeground; }
            set
            {
                _saveButtonForeground = value;
                OnPropertyChanged(() => SaveButtonForeground);
            }
        }

        private string _saveButtonBackground = Preferences.HyperlinkBackground;
        public string SaveButtonBackground
        {
            get { return _saveButtonBackground; }
            set
            {
                _saveButtonBackground = value;
                OnPropertyChanged(() => SaveButtonBackground);
            }
        }

        private string _saveButtonText = "Save";
        public string SaveButtonText
        {
            get { return _saveButtonText; }
            set
            {
                _saveButtonText = value;
                OnPropertyChanged(() => SaveButtonText);
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

        public StaffViewModel(string id)
        {
            Staff = Utils.GetStaff(Guid.Parse(id));
            Width = Preferences.MeasureWidth * Defaults.DefaultMeasureDensity;

            //TODO. Do we need to hide these hyperlinks here? We're gonna find out.
            //TransposeHyperlinkVisibility = Visibility.Collapsed;
            //ManageLyricsHyperlinkVisibility = Visibility.Collapsed;
            //PrintHyperlinkVisibility = Visibility.Collapsed;
            //SaveHyperlinkVisibility = Visibility.Collapsed;

            SetPropertiesForNewCompositionPanelDimensionControls();
            EA.GetEvent<AdjustBracketHeight>().Publish(string.Empty);

            SetHyperLinksVisibility();
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

        private void SetHyperLinksVisibility()
        {
            var sgSequence = Utils.GetStaffgroupSequence(Staff.Staffgroup_Id);
            if (sgSequence != 0) return;
            if (Staff.Sequence != 0) return;

            HyperlinksVisibility = Visibility.Visible;

            TransposeHyperlinkVisibility = Visibility.Visible;
            ManageLyricsHyperlinkVisibility = Visibility.Visible;
            PrintHyperlinkVisibility = Visibility.Visible;
            SaveHyperlinkVisibility = Visibility.Visible;

            ProvenanceHyperlinkVisibility = Visibility.Visible;
            AddStaffHyperlinkVisibility = Visibility.Visible;
            CollaborateHyperlinkVisibility = Visibility.Visible;
            HubHyperlinkVisibility = Visibility.Visible;
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
            ClickCommand = new DelegatedCommand<object>(OnClick);
            ClickPrint = new DelegatedCommand<object>(OnClickPrint);
            ClickSave = new DelegatedCommand<object>(OnClickSave);
            ClickTranspose = new DelegatedCommand<object>(OnClickTranspose);
            ClickHub = new DelegatedCommand<object>(OnClickHub);
            ClickProvenance = new DelegatedCommand<object>(OnClickProvenance);
            ClickCollaborate = new DelegatedCommand<object>(OnClickCollaborate);
            MouseMoveCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseMove, null);
            MouseLeftButtonUpCommand = new ExtendedDelegateCommand<ExtendedCommandParameter>(OnMouseLeftButtonUpCommand, null);
            ClickManageLyrics = new DelegatedCommand<object>(OnClickManageLyrics);
            AddStaff = new DelegatedCommand<object>(OnAddStaff);
        }

        private DelegatedCommand<object> _addStaff;
        public DelegatedCommand<object> AddStaff
        {
            get { return _addStaff; }
            set
            {
                _addStaff = value;
                OnPropertyChanged(() => AddStaff);
            }
        }

        private DelegatedCommand<object> _clickManageLyrics;
        public DelegatedCommand<object> ClickManageLyrics
        {
            get { return _clickManageLyrics; }
            set
            {
                _clickManageLyrics = value;
                OnPropertyChanged(() => ClickManageLyrics);
            }
        }

        public void OnClickManageLyrics(object obj)
        {
            if (EditorState.IsEditingLyrics)
            {
                EA.GetEvent<HideLyricsPanel>().Publish(true);
            }
            else
            {
                EA.GetEvent<ShowLyricsPanel>().Publish(true);
            }
        }

        private DelegatedCommand<object> _clickCollaborate;
        public DelegatedCommand<object> ClickCollaborate
        {
            get { return _clickCollaborate; }
            set
            {
                _clickCollaborate = value;
                OnPropertyChanged(() => ClickCollaborate);
            }
        }

        private DelegatedCommand<object> _clickProvenance;
        public DelegatedCommand<object> ClickProvenance
        {
            get { return _clickProvenance; }
            set
            {
                _clickProvenance = value;
                OnPropertyChanged(() => ClickProvenance);
            }
        }

        private DelegatedCommand<object> _clickTranspose;
        public DelegatedCommand<object> ClickTranspose
        {
            get { return _clickTranspose; }
            set
            {
                _clickTranspose = value;
                OnPropertyChanged(() => ClickTranspose);
            }
        }

        public void OnClickHub(object obj)
        {
            EA.GetEvent<ShowHub>().Publish(string.Empty);
        }

        public void OnClickSave(object obj)
        {
            EditorState.IsSaving = true;
            CompositionManager.DeleteUnusedContainers();
            EA.GetEvent<ShowProvenancePanel>().Publish(string.Empty);
            EA.GetEvent<ShowSavePanel>().Publish(string.Empty);
        }

        public void OnClickPrint(object obj)
        {
            CompositionManager.HideSocialChannels();
            //HARD CODED VALUE
            if (EditorState.GlobalStaffWidth > 925)
            {
                var scale = 925 / EditorState.GlobalStaffWidth;
                EA.GetEvent<ScaleViewportChanged>().Publish(scale);
            }
            EA.GetEvent<SetPrint>().Publish(string.Empty);
        }

        public void OnClickCollaborate(object obj)
        {
            if (EditorState.Collaborating)
            {
                EA.GetEvent<HideCollaborationPanel>().Publish(true);
            }
            else
            {
                EA.GetEvent<ShowCollaborationPanel>().Publish(true);
            }
        }

        public void OnClickProvenance(object obj)
        {
            if (EditorState.Provenancing)
            {
                EA.GetEvent<HideProvenancePanel>().Publish(false);
            }
            else
            {
                EA.GetEvent<ShowProvenancePanel>().Publish(false);
            }
        }

        public void OnClickTranspose(object obj)
        {
            if (EditorState.IsTransposing)
            {
                EA.GetEvent<HideTransposePanel>().Publish(true);
            }
            else
            {
                EA.GetEvent<ShowTransposePanel>().Publish(true);
            }
        }

        private DelegatedCommand<object> _clickAddStaff;
        public DelegatedCommand<object> ClickAddStaff
        {
            get { return _clickAddStaff; }
            set
            {
                _clickAddStaff = value;
                OnPropertyChanged(() => ClickAddStaff);
            }
        }

        private DelegatedCommand<object> _clickHub;
        public DelegatedCommand<object> ClickHub
        {
            get { return _clickHub; }
            set
            {
                _clickHub = value;
                OnPropertyChanged(() => ClickHub);
            }
        }
        private DelegatedCommand<object> _clickPrint;
        public DelegatedCommand<object> ClickPrint
        {
            get { return _clickPrint; }
            set
            {
                _clickPrint = value;
                OnPropertyChanged(() => ClickPrint);
            }
        }

        private DelegatedCommand<object> _clickSave;
        public DelegatedCommand<object> ClickSave
        {
            get { return _clickSave; }
            set
            {
                _clickSave = value;
                OnPropertyChanged(() => ClickSave);
            }
        }

        public void OnAddStaff(object obj)
        {
            EditorState.IsAddingStaffgroup = true;
            EA.GetEvent<UpdateMeasureBar>().Publish(Bars.StandardBarId);

            var staffgroup =
                StaffgroupManager.Create(CompositionManager.Composition.Id, CompositionManager.Composition.Staffgroups.Count() * Defaults.SequenceIncrement);

            var staffConfiguration = (_Enum.StaffConfiguration)CompositionManager.Composition.StaffConfiguration;

            var staffDensity = ((short)staffConfiguration == (short)_Enum.StaffConfiguration.Grand) ?
                Defaults.NewCompositionPanelGrandStaffConfigurationStaffDensity :
                Defaults.NewCompositionPanelSimpleStaffConfigurationStaffDensity;

            var idx = (from c in Cache.Measures select c.Index).Max();

            for (var index = 0; index < staffDensity; index++)
            {
                var staff = StaffManager.Create(staffgroup.Id, index * Defaults.SequenceIncrement);
                for (var midx = 0; midx < MeasureManager.CurrentDensity; midx++)
                {
                    var measure = MeasureManager.Create(staff.Id, midx * Defaults.SequenceIncrement);
                    measure.Index = idx += 1;
                    measure.Width = (from b in Cache.Measures where b.Index == midx select b.Width).Single();
                    staff.TimeSignature_Id = CompositionManager.Composition.TimeSignature_Id;
                    staff.Key_Id = (short)CompositionManager.Composition.Key_Id;
                    staff.Measures.Add(measure);
                    Cache.AddMeasure(measure);
                }
                staff.Clef_Id = CompositionManager.ClefIds[index];
                Cache.AddStaff(staff);
                staffgroup.Key_Id = (short)CompositionManager.Composition.Key_Id;
                staffgroup.Staffs.Add(staff);
            }
            StaffgroupManager.CurrentDensity++;
            Infrastructure.Support.Densities.StaffgroupDensity++;
            EA.GetEvent<UpdateMeasurePackState>().Publish(new Tuple<Guid, _Enum.EntityFilter>(staffgroup.Id, _Enum.EntityFilter.Staffgroup));
            CompositionManager.Composition.Staffgroups.Add(staffgroup);
            Cache.AddStaffgroup(staffgroup);
            EA.GetEvent<UpdateComposition>().Publish(CompositionManager.Composition);
            EditorState.IsAddingStaffgroup = false;
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
            EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
            EA.GetEvent<ToggleHyperlinkVisibility>().Subscribe(OnToggleHyperlinkVisibility);
            EA.GetEvent<UpdateSaveButtonHyperlink>().Subscribe(OnUpdateSaveButtonHyperlink);
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

        public void OnUpdateSaveButtonHyperlink(object obj)
        {
            SaveButtonText = (EditorState.Dirty) ? "Save Changes" : "Save";
            SaveButtonForeground = (EditorState.Dirty) ? "#FFFFFF" : Preferences.HyperlinkForeground;
            SaveButtonBackground = (EditorState.Dirty) ? Preferences.SelectorColor : "Transparent";
        }

        public void OnResumeEditing(object obj)
        {
            EA.GetEvent<ToggleHyperlinkVisibility>().Publish(new Tuple<Visibility, _Enum.HyperlinkButton>(Visibility.Visible, _Enum.HyperlinkButton.All));
        }

        public void OnToggleHyperlinkVisibility(Tuple<Visibility, _Enum.HyperlinkButton> payload)
        {
            switch (payload.Item2)
            {
                case _Enum.HyperlinkButton.Print:
                    PrintHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.Lyrics:
                    ManageLyricsHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.Save:
                    SaveHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.Transpose:
                    TransposeHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.Provenance:
                    ProvenanceHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.Collaboration:
                    CollaborateHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.AddStaff:
                    AddStaffHyperlinkVisibility = payload.Item1;
                    break;
                case _Enum.HyperlinkButton.All:
                    AddStaffHyperlinkVisibility = payload.Item1;
                    CollaborateHyperlinkVisibility = payload.Item1;
                    ProvenanceHyperlinkVisibility = payload.Item1;
                    TransposeHyperlinkVisibility = payload.Item1;
                    SaveHyperlinkVisibility = payload.Item1;
                    ManageLyricsHyperlinkVisibility = payload.Item1;
                    PrintHyperlinkVisibility = payload.Item1;
                    break;
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
            var sequence = Utils.GetStaffgroupSequence(Staff.Staffgroup_Id);
            if (sequence != 0 || Staff.Sequence != 0) return;
            // verse numbers only visible in first staff of first staffgroup.
            VerseNumbersVisibility = Visibility.Visible;
            VerseIndexes = new List<int>();
            for (var i = 1; i <= verseCount; i++)
            {
                VerseIndexes.Add(i);
            }
        }

        private Visibility _hyperlinksVisibility = Visibility.Collapsed;
        public Visibility HyperlinksVisibility
        {
            get { return _hyperlinksVisibility; }
            set
            {
                _hyperlinksVisibility = value;
                OnPropertyChanged(() => HyperlinksVisibility);
            }
        }

        private Visibility _collaborateHyperlinkVisibility = Visibility.Collapsed;
        public Visibility CollaborateHyperlinkVisibility
        {
            get { return _collaborateHyperlinkVisibility; }
            set
            {
                _collaborateHyperlinkVisibility = value;
                OnPropertyChanged(() => CollaborateHyperlinkVisibility);
            }
        }

        private Visibility _hubHyperlinkVisibility = Visibility.Collapsed;
        public Visibility HubHyperlinkVisibility
        {
            get { return _hubHyperlinkVisibility; }
            set
            {
                _hubHyperlinkVisibility = value;
                OnPropertyChanged(() => HubHyperlinkVisibility);
            }
        }

        private Visibility _transposeHyperlinkVisibility = Visibility.Collapsed;
        public Visibility TransposeHyperlinkVisibility
        {
            get { return _transposeHyperlinkVisibility; }
            set
            {
                _transposeHyperlinkVisibility = value;
                OnPropertyChanged(() => TransposeHyperlinkVisibility);
            }
        }

        private Visibility _addStaffHyperlinkVisibility = Visibility.Collapsed;
        public Visibility AddStaffHyperlinkVisibility
        {
            get { return _addStaffHyperlinkVisibility; }
            set
            {
                _addStaffHyperlinkVisibility = value;
                OnPropertyChanged(() => AddStaffHyperlinkVisibility);
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

        private Visibility _saveHyperlinkVisibility = Visibility.Collapsed;
        public Visibility SaveHyperlinkVisibility
        {
            get { return _saveHyperlinkVisibility; }
            set
            {
                _saveHyperlinkVisibility = value;
                OnPropertyChanged(() => SaveHyperlinkVisibility);
            }
        }

        private Visibility _printHyperlinkVisibility = Visibility.Collapsed;
        public Visibility PrintHyperlinkVisibility
        {
            get { return _printHyperlinkVisibility; }
            set
            {
                _printHyperlinkVisibility = value;
                OnPropertyChanged(() => PrintHyperlinkVisibility);
            }
        }

        private Visibility _manageLyricsHyperlinkVisibility = Visibility.Collapsed;
        public Visibility ManageLyricsHyperlinkVisibility
        {
            get { return _manageLyricsHyperlinkVisibility; }
            set
            {
                _manageLyricsHyperlinkVisibility = value;
                OnPropertyChanged(() => ManageLyricsHyperlinkVisibility);
            }
        }

        private Visibility _provenanceHyperlinkVisibility = Visibility.Collapsed;
        public Visibility ProvenanceHyperlinkVisibility
        {
            get { return _provenanceHyperlinkVisibility; }
            set
            {
                _provenanceHyperlinkVisibility = value;
                OnPropertyChanged(() => ProvenanceHyperlinkVisibility);
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
    }
}