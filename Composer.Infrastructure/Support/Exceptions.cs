using System;
using System.Globalization;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using System.Diagnostics;

namespace Composer.Infrastructure
{
    public static class Exceptions
    {
        private static readonly IEventAggregator Ea;

        private static bool _exceptionAppendAppendStackTrace = false;
        private static bool _exceptionRecurseError = false;
        private static bool _exceptionDisplayFriendlyErrors = true;
        private static bool _exceptionUseReflection = true;

        static Exceptions()
        {
            try
            {
                if (ServiceLocator.Current != null)
                {
                    Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Exception in Constructor: " + ex.Message));
            }
        }

        public static void HandleException(string message)
        {
            try
            {
                if (message.Length > 0)
                {
                    Ea.GetEvent<DisplayExceptionMessage>().Publish(message);
                    Ea.GetEvent<HideBusyIndicator>().Publish(string.Empty);
                }
            }
            catch (Exception ex)
            {
                throw (new Exception("Exception in HandleException(string message): " + ex.Message));
            }
        }

        public static void HandleException(Exception ex)
        {
            HandleException(ex, string.Empty);
        }

        public static void HandleException(Exception ex, string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                msg = string.Format("{0} {1}", ex.Message, (ex.InnerException == null) ? "" : ex.InnerException.Message);
            }
            try
            {
                var message = (_exceptionRecurseError) ? msg + RecurseErrorStack(ex) : msg;

                if (message.Length > 0)
                {
                    Ea.GetEvent<DisplayExceptionMessage>().Publish(message);
                    Ea.GetEvent<HideBusyIndicator>().Publish(string.Empty);
                }
            }
            catch (Exception iex)
            {
                throw (new Exception("Exception in HandleException(Exception ex, string msg): " + iex.Message));
            }
        }

        public static string RecurseErrorStack(Exception ex)
        {
            string sbError = String.Empty;
            if (ex != null)
            {
                string stackTrace = (_exceptionAppendAppendStackTrace) ? ex.StackTrace : string.Empty;
                sbError = String.Format(CultureInfo.CurrentUICulture, "ERROR: '{0}'{1}STACK: {2}{3}", ex.Message, Environment.NewLine, Environment.NewLine, stackTrace);
                sbError += RecurseErrorStack(ex.InnerException);
                sbError += ex.StackTrace;
            }
            return sbError;
        }
    }
}