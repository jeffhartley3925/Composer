using Composer.Modules.Composition.EventArgs;
using System;

namespace Composer.Modules.Composition.Service
{
    public interface IHubCompositionsService
    {
        void GetHubCompositionsAsync();

        event EventHandler<HubCompositionsLoadingEventArgs> HubCompositionsLoadingComplete;
        event EventHandler<HubCompositionsErrorEventArgs> HubCompositionsLoadingError;
    }
}
