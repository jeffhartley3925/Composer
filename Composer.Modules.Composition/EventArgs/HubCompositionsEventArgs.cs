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

namespace Composer.Modules.Composition.EventArgs
{
    public class HubCompositionsLoadingEventArgs : System.EventArgs
    {
        public IEnumerable<Composer.Repository.DataService.Composition> Results { get; private set; }

        public HubCompositionsLoadingEventArgs(IEnumerable<Composer.Repository.DataService.Composition> results)
        {
            Results = results;
        }
    }
    public class HubCompositionsErrorEventArgs : System.EventArgs
    {
        private Exception exception = null;

        public HubCompositionsErrorEventArgs(Exception exception)
        {
            this.exception = exception;
        }
        public Exception Error
        {
            get { return exception; }
        }
    }
}