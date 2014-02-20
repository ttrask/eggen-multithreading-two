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
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class Record
    {
        [System.ComponentModel.DataAnnotations.KeyAttribute]
        public Int64 RecordId { get; set; }
        public System.DateTime CreateDtTm { get;  private set;}
        public string CreateUser { get; set; }

        [ForeignKey("DropFile")]
        public Int64 DropFileId { get; set; }
        public int Size { get; set; }
        public int ProcessorCount { get; set; }
        public System.DateTime ExecuteDtTm { get; set; }
        public decimal TimeValue { get; set; }

        [ForeignKey("TimeType")]
        public Int64 TimeTypeId { get; set; }
        [ForeignKey("Language")]
        public Int64 LanguageId { get; set; }
        [ForeignKey("Environment")]
        public Int64 EnvironmentId { get; set; }
    
        public virtual DropFile DropFile { get; set; }
        public virtual Environment Environment { get; set; }
        public virtual Language Language { get; set; }
        public virtual TimeType TimeType { get; set; }
    }
}