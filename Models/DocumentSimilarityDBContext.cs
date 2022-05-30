using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.Models
{
    public class DocumentSimilarityDBContext : DbContext
    {
        public DocumentSimilarityDBContext() : base("DocumentSimilarityDBContext") { }


        public DbSet<ResearchDocument> ResearchDocuments { get; set; }
        public DbSet<Author> Writers { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Specifying singular table names (Papers or paper? in this case the table name is paper)
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }

}