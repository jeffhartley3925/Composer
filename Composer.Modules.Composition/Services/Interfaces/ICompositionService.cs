using System;
using Composer.Modules.Composition.EventArgs;

namespace Composer.Modules.Composition.Service
{
    public interface ICompositionService
    {
        void GetCompositionAsync();
        Repository.DataService.Composition Composition { get; set; }

        event EventHandler<CompositionLoadingEventArgs> CompositionLoadingComplete;
        event EventHandler<CompositionErrorEventArgs> CompositionLoadingError;
    }
}
