//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppStats.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Time
    {
        public int RecordId { get; set; }
        public decimal TimeValue { get; set; }
        public byte TimeTypeId { get; set; }
        [System.ComponentModel.DataAnnotations.KeyAttribute]
        public long TimeId { get; set; }
    
        public virtual Record Record { get; set; }
        public virtual Time Times1 { get; set; }
        public virtual Time Time1 { get; set; }
        public virtual TimeType TimeType { get; set; }
    }
}