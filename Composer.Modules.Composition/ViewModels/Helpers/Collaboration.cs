using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Composer.Infrastructure.Constants;
using Composer.Infrastructure.Support;
using Composer.Modules.Composition.Converters;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.ViewModels
{
    public class Collaboration
    {
        public Guid Key { get; set; }
        public int Index { get; set; }
        public string Author_Id { get; set; }
        public string Collaborator_Id { get; set; }
        public string Name { get; set; }
        public Guid Composition_Id { get; set; }
        public bool IsSelfSame { get; set; }
    }
}