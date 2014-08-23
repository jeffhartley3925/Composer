using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Models
{
    public sealed class Measuregroup
    {
        public Guid Id { get; set; }
        public Guid StaffgroupId { get; set; }

        public double Starttime { get; set; }
        public Sequencegroup Sequencegroup { get; set; }
        public int Index { get; set; }

        public IEnumerable<Repository.DataService.Measure> Measures 
        { 
            get; 
            set; 
        }

        public Measuregroup(Guid staffgroupId, int sequence, int index)
        {
            this.Id = Guid.NewGuid();
            this.Index = index;
            this.StaffgroupId = staffgroupId;
        }
    }
}
