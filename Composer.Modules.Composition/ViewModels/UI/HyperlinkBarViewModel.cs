using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Composer.Infrastructure;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Dimensions;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition.ViewModels.UI
{
    public class HyperlinkBarViewModel : BaseViewModel, IHyperlinkBarViewModel, IEventCatcher
    {
        public HyperlinkBarViewModel()
        {
            SetHyperLinksVisibility();
            DefineCommands();
            SubscribeEvents();
        }
        private void SetHyperLinksVisibility()
        {
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

        public void SubscribeEvents()
        {
            EA.GetEvent<ToggleHyperlinkVisibility>().Subscribe(OnToggleHyperlinkVisibility);
            EA.GetEvent<UpdateSaveButtonHyperlink>().Subscribe(OnUpdateSaveButtonHyperlink);
            EA.GetEvent<ResumeEditing>().Subscribe(OnResumeEditing);
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
                    HubHyperlinkVisibility = payload.Item1;
                    CollaborateHyperlinkVisibility = payload.Item1;
                    ProvenanceHyperlinkVisibility = payload.Item1;
                    TransposeHyperlinkVisibility = payload.Item1;
                    SaveHyperlinkVisibility = payload.Item1;
                    ManageLyricsHyperlinkVisibility = payload.Item1;
                    PrintHyperlinkVisibility = payload.Item1;
                    break;
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
            ClickManageLyrics = new DelegatedCommand<object>(OnClickManageLyrics);
            AddStaff = new DelegatedCommand<object>(OnAddStaff);
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
            SequenceManager.Spinup();
            MeasuregroupManager.Spinup();
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

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}
