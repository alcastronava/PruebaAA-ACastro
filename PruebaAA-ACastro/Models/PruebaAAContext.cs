using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PruebaAA_ACastro.Models
{
    class PruebaAAContext : DbContext
    {
        private const string connectionString = "Server=ALFREDO-PC\\SQLEXPRESS;Database=PruebaAAACastro;Trusted_Connection=True;";

        public PruebaAAContext()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<Stock> Stocks { get; set; }
    }
}
