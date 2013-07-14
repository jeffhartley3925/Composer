using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Composer.Modules.Composition.EventArgs;

namespace Composer.Modules.Composition.Service
{
    public interface IDialogsService
    {
        void GetCompositions();

        event EventHandler<CompositionLoadingEventArgs> CompositionListLoadingComplete;
        event EventHandler<CompositionErrorEventArgs> CompositionPickerLoadingError;
    }
}
