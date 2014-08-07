using System;

namespace Composer.Infrastructure.Events
{
    public interface IEventCatcher
    {
        void SubscribeEvents();
        void DefineCommands();
        bool IsTargetVM(Guid Id);
    }
}
