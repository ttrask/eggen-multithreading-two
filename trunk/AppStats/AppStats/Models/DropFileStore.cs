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

    public partial class DropFileStore
    {
        public byte[] DropFileRawData { get; set; }

        [ForeignKey("DropFile")]
        public Int64 DropFileId { get; set; }
        [System.ComponentModel.DataAnnotations.KeyAttribute]
        public Int64 DropFileStoreId { get; set; }
    
        public virtual DropFile DropFile { get; set; }
    }
}
