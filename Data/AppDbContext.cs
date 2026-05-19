using Microsoft.EntityFrameworkCore;
using CCSMonitoringSystem.Models;

namespace CCSMonitoringSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IdNumber is the primary key
            modelBuilder.Entity<Student>()
                .HasKey(s => s.IdNumber);
        }
    }
}
