//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Composer.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Chord
    {
        public Chord()
        {
            this.Notes = new HashSet<Note>();
            this.Audit = new Audit();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid Measure_Id { get; set; }
        public short Key_Id { get; set; }
        public int Location_X { get; set; }
        public int Location_Y { get; set; }
        public Nullable<double> StartTime { get; set; }
        public decimal Duration { get; set; }
        public string Status { get; set; }
    
        public Audit Audit { get; set; }
    
        public virtual ICollection<Note> Notes { get; set; }
        public virtual Measure Measure { get; set; }
    }
}
