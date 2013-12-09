using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using AppStats.Models;


namespace AppStats.DataAccess
{
    public class AppStatsContext: DbContext
    {
        public DbSet<Record> Records { get; set; }
        public DbSet<AppStats.Models.Environment> Environments { get; set; }
        public DbSet<Language> Languages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

    }
}