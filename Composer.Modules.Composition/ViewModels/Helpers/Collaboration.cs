using System;

namespace Composer.Modules.Composition.ViewModels
{
    public class Collaboration
    {
        public Guid Key { get; set; }
        public int Index { get; set; }
        public string Author_Id { get; set; }
        public string CollaboratorId { get; set; }
        public string Name { get; set; }
        public Guid Composition_Id { get; set; }
        public bool IsSelfSame { get; set; }
    }
}