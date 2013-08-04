using Composer.Modules.Composition.EventArgs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Modules.Composition.Service
{
    public interface IHubCompositionsService
    {
        void GetHubCompositionsAsync();

        event EventHandler<HubCompositionsLoadingEventArgs> HubCompositionsLoadingComplete;
        event EventHandler<HubCompositionsErrorEventArgs> HubCompositionsLoadingError;
    }
}
