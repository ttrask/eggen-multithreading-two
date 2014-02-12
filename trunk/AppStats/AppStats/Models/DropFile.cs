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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public partial class DropFile
    {
        public DropFile()
        {
            this.Records = new HashSet<Record>();
            this.DropFileStores = new HashSet<DropFileStore>();
        }

        [KeyAttribute]
        public Int64 DropFileId { get; set; }
        public string Filename { get; set; }
        public System.DateTime CraeteDtTm { get; set; }
        public bool IsActive { get; set; }
        
        [ForeignKey("Language")]
        public Int64 LanguageId { get; set; }

        [ForeignKey("Environment")]
        public Int64 EnvironmentId { get; set; }

        
        public virtual Environment Environment { get; set; }
        public virtual Language Language { get; set; }
        public virtual ICollection<Record> Records { get; set; }
        public virtual ICollection<DropFileStore> DropFileStores { get; set; }
    }
}
