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

namespace Composer.Infrastructure
{
    public class ProvenancePayload
    {
        public string TitleLine { get; set; }
        public DateTime CreateDate { get; set; }
         public List<string> AuthorIds { get; set; }
    }
}
