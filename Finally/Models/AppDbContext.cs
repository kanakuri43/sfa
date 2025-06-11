using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace Finally.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<ProgressLevel> ProgressLevels { get; set; }
        public DbSet<Calendar> Calendars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = LoadConfig();
            string connectionString = (config.ConnectionString).ToString();
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Employee>()
                  .ToTable("M社員")
                  .HasKey(s => new { s.Code });

            modelBuilder.Entity<Section>()
                  .ToTable("M部門")
                  .HasKey(s => new { s.Code });

            modelBuilder.Entity<Calendar>()
                  .ToTable("Mカレンダ")
                  .HasKey(c => new { c.Date });


        }


        static dynamic LoadConfig()
        {
            var doc = XDocument.Load("config.xml");

            return new
            {
                ConnectionString = doc.Root.Element("Database").Element("ConnectionString").Value,
            };
        }
    }
}
