using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Composer.Infrastructure.Support;
using Composer.Infrastructure.Constants;
using Composer.Repository;

namespace Composer.Modules.Composition.ViewModels
{
    public class Collaborator
    {
        public Guid Key { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public string AuthorId { get; set; }
        public int PendingAdds { get; set; }
        public int PendingDeletes { get; set; }
        public int AcceptedContributions { get; set; }
        public int RejectedContributions { get; set; }

        public Collaborator(string name, string id)
        {
            Name = name;
            AuthorId = id;
            Index = (from a in Collaborations.CurrentCollaborations where a.CollaboratorId == id select a.Index).First();
            Key = (from a in Collaborations.CurrentCollaborations where a.CollaboratorId == id select a.Key).First();
        }
    }
}
