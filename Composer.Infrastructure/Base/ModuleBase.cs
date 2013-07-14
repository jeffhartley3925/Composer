using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Infrastructure
{
    public abstract class ModuleBase : IModule
    {
        protected IEventAggregator EA { get; set; }

        public IUnityContainer Container { get; set; }

        protected ModuleBase(IUnityContainer container)
        {
            if (container != null) Container = container;
            EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        public virtual void Initialize()
        {
            RegisterViewsAndServices();
        }

        protected abstract void RegisterViewsAndServices();
    }
}