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
    
    public partial class Measure
    {
        public Measure()
        {
            this.Chords = new HashSet<Chord>();
            this.Audit = new Audit();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid Staff_Id { get; set; }
        public Nullable<int> TimeSignature_Id { get; set; }
        public int Instrument_Id { get; set; }
        public short Bar_Id { get; set; }
        public short Key_Id { get; set; }
        public string Width { get; set; }
        public decimal Duration { get; set; }
        public string LedgerColor { get; set; }
        public int Sequence { get; set; }
        public short Index { get; set; }
        public int Spacing { get; set; }
        public string Status { get; set; }
    
        public Audit Audit { get; set; }
    
        public virtual Staff Staff { get; set; }
        public virtual ICollection<Chord> Chords { get; set; }
    }
}
