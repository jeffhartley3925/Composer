using Composer.Infrastructure;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Regions;
using Composer.Modules.Palettes.Services;
using Composer.Modules.Palettes.Views;
using Composer.Modules.Palettes.ViewModels;
using Composer.Infrastructure.Events;
using Microsoft.Practices.ServiceLocation;
namespace Composer.Modules.Palettes
{
    public class PalettesModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private PaletteManager _paletteManager;
        public PalettesModule(IUnityContainer container, IRegionManager regionManager)
            : base(container)
        {
            _regionManager = regionManager;
            SubscribeEvents();
        }
        public void SubscribeEvents()
        {
            EA.GetEvent<CompositionLoaded>().Subscribe(LoadPalettes, true);
            EA.GetEvent<NewComposition>().Subscribe(LoadPalettes, true);
            EA.GetEvent<EditorStateChanged>().Subscribe(OnEditorStateChanged, true);
        }
        public void OnEditorStateChanged(object obj)
        {
            if (_paletteManager == null)
                _paletteManager = ServiceLocator.Current.GetInstance<PaletteManager>();
            _paletteManager.UpdateState(obj);
        }

        protected override void RegisterViewsAndServices()
        {
            Container.RegisterType<IDurationsService, DurationsService>("DurationsService", new ContainerControlledLifetimeManager());
            Container.RegisterType<IToolsService, ToolsService>("ToolsService", new ContainerControlledLifetimeManager());
            //Container.RegisterType<IPlaybackControlsService, PlaybackControlsService>("PlaybackControlsService", new ContainerControlledLifetimeManager());

            Container.RegisterType<IDurationsView, DurationsView>();
            Container.RegisterType<IToolsView, ToolsView>();
            Container.RegisterType<IPlaybackControlsView, PlaybackControlsView>();

            Container.RegisterType<IToolsViewModel, ToolsViewModel>("ToolsViewModel", new ContainerControlledLifetimeManager());
            Container.RegisterType<IDurationsViewModel, DurationsViewModel>();
            //Container.RegisterType<IPlaybackViewModel, PlaybackViewModel>("PlaybackViewModel", new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(PaletteManager), new PaletteManager(), new ContainerControlledLifetimeManager());
        }

        public void LoadPalettes(object obj)
        {
            _regionManager.RegisterViewWithRegion("DurationsRegion", () => Container.Resolve<IDurationsView>());
            _regionManager.RegisterViewWithRegion("ToolsRegion", () => Container.Resolve<IToolsView>());
            //_regionManager.RegisterViewWithRegion("PlaybackControlsRegion", () => Container.Resolve<IPlaybackControlsView>());
        }
    }
}
