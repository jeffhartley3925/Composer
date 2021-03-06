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
    
    public partial class Arc
    {
        public Arc()
        {
            this.Audit = new Audit();
        }
    
        public System.Guid Id { get; set; }
        public System.Guid Composition_Id { get; set; }
        public System.Guid Note_Id1 { get; set; }
        public System.Guid Note_Id2 { get; set; }
        public System.Guid Chord_Id1 { get; set; }
        public System.Guid Chord_Id2 { get; set; }
        public short Type { get; set; }
        public string Status { get; set; }
        public string ArcSweep { get; set; }
        public string FlareSweep { get; set; }
        public Nullable<double> Angle { get; set; }
        public Nullable<short> X1 { get; set; }
        public Nullable<short> Y1 { get; set; }
        public Nullable<short> X2 { get; set; }
        public Nullable<short> Y2 { get; set; }
        public double Top { get; set; }
        public Nullable<double> Left { get; set; }
        public System.Guid Staff_Id { get; set; }
    
        public Audit Audit { get; set; }
    }
}
