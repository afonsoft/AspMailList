﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AspMailList.Service
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class afonsoftcombr_dbEntities : DbContext
    {
        public afonsoftcombr_dbEntities()
            : base("name=afonsoftcombr_dbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<mala_direta> mala_direta { get; set; }
        public DbSet<mala_direta_campanha> mala_direta_campanha { get; set; }
        public DbSet<mala_direta_campanha_enviado> mala_direta_campanha_enviado { get; set; }
        public DbSet<mala_direta_campanha_smtp_mail> mala_direta_campanha_smtp_mail { get; set; }
        public DbSet<mala_direta_campanha_unsubscribe> mala_direta_campanha_unsubscribe { get; set; }
    }
}
