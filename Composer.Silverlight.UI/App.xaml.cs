using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using Composer.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Composite.Events;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Silverlight.UI
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();
            Composer.Infrastructure.Converters.ConverStyleIdToStyleValue converter = new Composer.Infrastructure.Converters.ConverStyleIdToStyleValue();
        }
        private string cid = string.Empty;
        private string uid = string.Empty;
        private string mid = string.Empty;
        private string cindex = "0"; //hard-code is OK here. 

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ProcessQueryString();
            Repository.Host.Value = HtmlPage.Document.DocumentUri.Host.Trim().ToLower();
            Repository.Host.Application = "composer";
            Repository.Host.CompositionFileDirectory = "compositionFiles";
            Repository.Host.CompositionImageDirectory = "compositionImages";
            Bootstrapper bootStrapper = new Bootstrapper();
            bootStrapper.Run();
            if (e.InitParams != null)
            {
                foreach (var item in e.InitParams)
                {
                    this.Resources.Add(item.Key, item.Value);
                }
            }
        }

        private void ProcessQueryString()
        {
            //TODO: redundancy. both QueryString and EditorState storing query-string data?
            if (HtmlPage.Document.QueryString.ContainsKey("id"))
            {
                cid = HtmlPage.Document.QueryString["id"].ToString();
                QueryString.compositionId = cid;
                if (HtmlPage.Document.QueryString.ContainsKey("index"))
                {
                    cindex = HtmlPage.Document.QueryString["index"].ToString();
                }
                QueryString.collaborationIndex = cindex;
            }
            if (HtmlPage.Document.QueryString.ContainsKey("UID"))
            {
                uid = HtmlPage.Document.QueryString["UID"].ToString();
                QueryString.UserId = uid;
            }
            if (HtmlPage.Document.QueryString.ContainsKey("MID"))
            {
                mid = HtmlPage.Document.QueryString["MID"].ToString();
                QueryString.measureId = mid;
            }
            QueryString.Initialize();

            Composer.Infrastructure.EditorState.qsId = QueryString.CompositionId;
            Composer.Infrastructure.EditorState.qsIndex = QueryString.collaborationIndex.ToString();
            Composer.Infrastructure.EditorState.MidQueryString = QueryString.MeasureId;
            Composer.Infrastructure.EditorState.UidQueryString = QueryString.UserId;
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                //Composer.Infrastructure.Exceptions.HandleException(e.ExceptionObject);
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\note", @"\note");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception ex)
            {
                // could cause infinite loop.
                // Exceptions.HandleException(ex, "ReportErrorToDOM");
            }
        }
    }
}
