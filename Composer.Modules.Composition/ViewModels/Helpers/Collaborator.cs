using System;
using System.Linq;

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
