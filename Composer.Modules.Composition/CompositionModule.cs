using Composer.Infrastructure;
using System.Linq;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Service;
using Composer.Modules.Composition.ViewModels;
using Composer.Modules.Composition.Views;
using Composer.Repository;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Composer.Infrastructure.Dimensions;
using System.Windows;
using Composer.Modules.Composition.ViewModels.Helpers;
using System;

namespace Composer.Modules.Composition
{
    public sealed class CompositionModule : ModuleBase
    {
        private readonly IRegionManager _rm;
        private CompositionService _service;
        private DataServiceRepository<Repository.DataService.Composition> _r;
        public CompositionModule(IUnityContainer container, IRegionManager regionManager)
            : base(container)
        {
            _rm = regionManager;
            DefineCommands();
            SubscribeEvents();
            EditorState.Reset();
        }

        private void SetRepository()
        {
            if (_r == null)
                _r = ServiceLocator.Current.GetInstance<DataServiceRepository<Repository.DataService.Composition>>();
        }

        public void DefineCommands()
        {
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<NewComposition>().Subscribe(OnNewComposition, true);
            EA.GetEvent<LoadComposition>().Subscribe(OnLoadComposition, true);

			EA.GetEvent<SetPrint>().Subscribe(OnSetPrint, true);
            EA.GetEvent<SetProvenancePanel>().Subscribe(OnSetNewCompositionPanelProvenance, true);
			EA.GetEvent<ClosePrintPreview>().Subscribe(OnRemovePrint, true);
            EA.GetEvent<Login>().Subscribe(OnLogin, true);
            EA.GetEvent<UpdateStaffDimensionWidth>().Subscribe(OnUpdateStaffDimensionAreaWidth, true);
        }

        private void OnUpdateStaffDimensionAreaWidth(short keyId)
        {
            //why is this method in this class? there are 2 publishers of this event. they both instantiated early, so we need to 
            //have a subscriber declare itself early so the published events have a place to go. this class is the first to load, so the subscription is here.

            //TODO: here and many other locations, create static functions in all dimension class for getting at these values.
            //for example - string accidental = Keys.GetAccidentalFromKeyShortName(shortName);
            var key = (from a in Keys.KeyList where a.Id == keyId select a).Single().Name;
            var accidental = (from a in Keys.keyScaleMap where a.ShortName == key select a).Single().Accidental;
            var count = (from a in Keys.KeyList where a.Id == keyId select a).Single().AccidentalCount;
            if (count != null)
            {
                var accidentalCount = (int)count;

                EditorState.AccidentalCount = accidentalCount;

                switch (accidental)
                {
                    case Defaults.AccidentalFlatSymbol:
                        EditorState.AccidentalWidth = Defaults.AccidentalFlatWidth;
                        break;
                    case Defaults.AccidentalSharpSymbol:
                        EditorState.AccidentalWidth = Defaults.AccidentalSharpWidth;
                        break;
                    default:
                        EditorState.AccidentalWidth = 0;
                        break;
                }

                var staffDimensionKeyWidth = Math.Max(accidentalCount * EditorState.AccidentalWidth - 1, EditorState.AccidentalWidth);
                //width of dimension area varies with the number of accidentals in the key signature. this method calculates that width, then
                //adds in constant widths for other dimension area elements to get the actual width. we need this exact width when calculating
                //arc y coordinates.
                Preferences.StaffDimensionAreaWidth = 
                    Defaults.StaffDimensionClefWidth + 
                    Defaults.StaffDimensionSpacerWidth + 
                    staffDimensionKeyWidth + 
                    Defaults.StaffDimensionSpacerWidth + 
                    Defaults.StaffDimensionTimeSignatureWidth;
            }
        }

        protected override void RegisterViewsAndServices()
        {
            Container.RegisterType<ICompositionViewModel, CompositionViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IBarsView, BarsView>();
			Container.RegisterType<IPrintView, PrintView>();
            Container.RegisterType<IUIScaleView, UIScaleView>();
            Container.RegisterType<IProvenanceView, ProvenanceView>();
            Container.RegisterType<IBusyIndicatorView, BusyIndicatorView>();
            Container.RegisterType<IEditPopupView, EditPopupView>();
            Container.RegisterType<INoteEditorView, NoteEditorView>();
            Container.RegisterType<IPlaybackControlsView, PlaybackControlsView>();
            Container.RegisterType<ILyricsPanelView, LyricsPanelView>();
            Container.RegisterType<IPlaybackControlsViewModel, PlaybackControlsViewModel>();
            Container.RegisterType<ISavePanelView, SavePanelView>();
            Container.RegisterType<ICollaborationPanelView, CollaborationPanelView>();
            Container.RegisterType<ITranspositionView, TranspositionView>();
            Container.RegisterType<IHyperlinkBarView, HyperlinkBarView>();
            Container.RegisterType<INewCompositionPanelView, NewCompositionPanelView>();
            Container.RegisterInstance(typeof(ICompositionService), new CompositionService(), new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(IHubCompositionsService), new HubCompositionsService(), new ContainerControlledLifetimeManager());

            Container.RegisterType<IHubView, HubView>();

            _service = (CompositionService)Container.Resolve<ICompositionService>();

            Container.RegisterInstance(typeof(BarsViewModel), new BarsViewModel(), new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(ProvenanceViewModel), new ProvenanceViewModel(), new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(DataServiceRepository<Repository.DataService.Composition>), new DataServiceRepository<Repository.DataService.Composition>(), new ContainerControlledLifetimeManager());
        }

        private void RegisterViewsWithRegions()
        {
            _rm.RegisterViewWithRegion(RegionNames.Bars, () => Container.Resolve<IBarsView>());
            _rm.RegisterViewWithRegion(RegionNames.BusyIndicator, () => Container.Resolve<IBusyIndicatorView>());
            _rm.RegisterViewWithRegion(RegionNames.UIScale, () => Container.Resolve<IUIScaleView>());
            _rm.RegisterViewWithRegion(RegionNames.Provenance, () => Container.Resolve<IProvenanceView>());
            _rm.RegisterViewWithRegion(RegionNames.EditPopup, () => Container.Resolve<IEditPopupView>());
            _rm.RegisterViewWithRegion(RegionNames.NoteEditor, () => Container.Resolve<INoteEditorView>());
            _rm.RegisterViewWithRegion(RegionNames.LyricsPanel, () => Container.Resolve<ILyricsPanelView>());
            _rm.RegisterViewWithRegion(RegionNames.HyperlinkBar, () => Container.Resolve<IHyperlinkBarView>());
            _rm.RegisterViewWithRegion(RegionNames.SavePanel, () => Container.Resolve<ISavePanelView>());
            _rm.RegisterViewWithRegion(RegionNames.Collaborations, () => Container.Resolve<ICollaborationPanelView>());
            _rm.RegisterViewWithRegion(RegionNames.Transposition, () => Container.Resolve<ITranspositionView>());
            _rm.RegisterViewWithRegion(RegionNames.PlaybackControls, () => Container.Resolve<IPlaybackControlsView>());
            _rm.RegisterViewWithRegion(RegionNames.Print, () => Container.Resolve<IPrintView>());
         }

		public void OnRemovePrint(object obj)
		{

		}

		public void OnSetPrint(object obj)
		{
			if (_rm.Regions.ContainsRegionWithName(RegionNames.Print))
			{
				_rm.RegisterViewWithRegion(RegionNames.Print, () => Container.Resolve<IPrintView>());
			}
		}

        public void OnSetNewCompositionPanelProvenance(object obj)
        {
            if (_rm.Regions.ContainsRegionWithName(RegionNames.Provenance))
            {
                _rm.RegisterViewWithRegion(RegionNames.Provenance, () => Container.Resolve<IProvenanceView>());
            }
        }

        public void OnNewComposition(Repository.DataService.Composition composition)
        {
            _service.Composition = composition;
            LoadCompositionView(composition);
            StaffManager.CurrentDensity = (Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.Simple ||
                                           Preferences.DefaultStaffConfiguration == _Enum.StaffConfiguration.None) ? Defaults.DefaultSimpleStaffStaffDensity : Defaults.DefaultGrandStaffStaffDensity;
            RegisterViewsWithRegions();
            InitializeDimensions();
            EA.GetEvent<ToggleSidebarVisibility>().Publish(Visibility.Visible);
            SetRepository();
            _r.Dirty = false;
        }

        public void OnLoadComposition(object obj)
        {
            EditorState.IsOpening = true;
            var composition = (Repository.DataService.Composition)obj;
            _service.CompositionId = composition.Id.ToString();
            LoadCompositionView(composition);
            StaffManager.CurrentDensity = (composition.StaffConfiguration == (int)_Enum.StaffConfiguration.Simple ||
                                           composition.StaffConfiguration == (int)_Enum.StaffConfiguration.None) ? Defaults.DefaultSimpleStaffStaffDensity : Defaults.DefaultGrandStaffStaffDensity;
            RegisterViewsWithRegions();
            InitializeDimensions();
            EA.GetEvent<ToggleSidebarVisibility>().Publish(Visibility.Visible);
            SetRepository();

            _r.Dirty = false;
        }

        private void LoadCompositionView(Repository.DataService.Composition c)
        {
            if (!Container.IsRegistered<ICompositionView>())
            {
                var r = _rm.Regions[RegionNames.Composition];
                Container.RegisterType<ICompositionView, CompositionView>(new ContainerControlledLifetimeManager());
                var v = (CompositionView)Container.Resolve<ICompositionView>();
                r.Add(v, "CompositionView", false);
            }
            else
            {
                var v = (CompositionView)Container.Resolve<ICompositionView>();
                v.DataContext = new CompositionViewModel(_service);
            }
            Cache.Initialize();
            CompositionManager.Composition = c;
        }

        private void InitializeDimensions()
        {
            Accidentals.Initialize();
            Keys.InitializeKeys();
            TimeSignatures.Initialize();
            Instruments.Initialize();
            Bars.Initialize();
            Clefs.Initialize();
            Infrastructure.Support.Vectors.Initialize();
            Playback.Initialize();

            EA.GetEvent<UpdateStaffDimensionWidth>().Publish((short)CompositionManager.Composition.Key_Id);
        }

        public void OnLogin(object obj)
        {
            EditorState.IsLoggedIn = true; 
            _rm.RegisterViewWithRegion(RegionNames.Hub, () => Container.Resolve<IHubView>());
            _rm.RegisterViewWithRegion(RegionNames.NewComposition, () => Container.Resolve<INewCompositionPanelView>());
        }
    }
}