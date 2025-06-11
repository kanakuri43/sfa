using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace Split.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<WeeklyProgress> WeeklyProgresses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<ProgressLevel> ProgressLevels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = LoadConfig();
            string connectionString = (config.ConnectionString).ToString();
            optionsBuilder.UseSqlServer(connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeeklyProgress>()
                  .ToTable("S進捗")
                  .HasKey(wp => new { wp.Date, wp.EmployeeCode, wp.ProgressType });

            modelBuilder.Entity<Target>()
                  .ToTable("S進捗目標")
                  .HasKey(t => new { t.YearMonth, t.SectionCode, t.EmployeeCode });

            modelBuilder.Entity<Employee>()
                  .ToTable("M社員")
                  .HasKey(s => new { s.Code });

            modelBuilder.Entity<Section>()
                  .ToTable("M部門")
                  .HasKey(s => new { s.Code });

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
