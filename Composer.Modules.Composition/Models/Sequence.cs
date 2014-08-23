using System;
using System.Collections.Generic;
using Composer.Modules.Composition.Annotations;
using Composer.Repository.DataService;

namespace Composer.Modules.Composition.Models
{
    public sealed class Sequencegroup
    {
        private Guid Id { [UsedImplicitly] get; set; }
		public List<Measure> Measures { get; set; }
        public List<Measuregroup> Measuregroups { get; set; }
        public double Starttime { get; set; }

        public int SequenceIndex { get; set; }

        public Sequencegroup(int sQiX)
        {
            this.Id = Guid.NewGuid();
            this.SequenceIndex = sQiX;
            Measuregroups = new List<Measuregroup>();
        }
    }
}
