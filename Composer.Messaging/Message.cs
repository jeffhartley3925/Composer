using System.IO;
namespace Composer.Messaging
{
    /// <summary>
    ///   Message class.
    ///   
    /// Some functionality requires CLR bits that aren't available within silverlight, so we 
    /// use WebClient (which is available to both silverlight and ASP.Net) to pass messages to 
    /// a ASP.Net controller and let the controller implement the functionality.
    /// 
    /// </summary>
    public class Message
    {
        private string compositionId = string.Empty;
        public string CompositionId
        {
            get { return compositionId; }
            set
            {
                compositionId = value;
            }
        }

        private string compositionTitle = string.Empty;
        public string CompositionTitle
        {
            get { return compositionTitle; }
            set
            {
                compositionTitle = value;
            }
        }

        private int? collaborationId = null;
        public int? CollaborationId
        {
            get { return collaborationId; }
            set
            {
                collaborationId = value;
            }
        }

        private string text = string.Empty;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
            }
        }
    }
}
