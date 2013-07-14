using System.Windows;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Browser;
using Composer.Infrastructure.Events;
using Composer.Infrastructure;

namespace Composer.Silverlight.UI
{
    public class JavascriptSilverlightBridge
    {
        private readonly IEventAggregator _ea;
        public JavascriptSilverlightBridge()
        {
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        [ScriptableMember]
        public void FinishedPlayback()
        {
            _ea.GetEvent<FinishedPlayback>().Publish(string.Empty);
            EditorState.ResumeStarttime = 0;
        }

        [ScriptableMember]
        public void SetResumeStarttime(double resumeStarttime)
        {
            EditorState.ResumeStarttime = resumeStarttime;
        }

        [ScriptableMember]
        public void ResizeViewport(double height, double width)
        {
            Height = height;
            Width = width;
            _ea.GetEvent<ResizeViewport>().Publish(new Point(width, height));
        }

        [ScriptableMember]
        public double Height { get; set; }

        private double _width;
        [ScriptableMember]
        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }
        [ScriptableMember]
        public void OnFacebookDependenciesLoaded(string dependencyCount, string loggedInUser) //not a aggregated event handler. invoked from index.js;
        {
            //loggedInUser and EditorState.idIdToUse used to make sure the correct fb profile info is loaded 
            //when EditorState.IsInternetAccess = false. Used for debugging in disconnected environment.

            EditorState.IdIdToUse =  (loggedInUser == "John Smith") ? 0 : 1;
            _ea.GetEvent<CheckFacebookDataLoaded>().Publish(string.Empty);
        }

        [ScriptableMember]
        public void OnDisplayMessage(string message)
        {
            _ea.GetEvent<DisplayMessage>().Publish(message);
        }
    }
}
