using Microsoft.EntityFrameworkCore;
using CCSMonitoringSystem.Models;

namespace CCSMonitoringSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<SitInSession> SitInSessions => Set<SitInSession>();
        public DbSet<Feedback> Feedback => Set<Feedback>();
        public DbSet<RewardPoint> RewardPoints => Set<RewardPoint>();
        public DbSet<NotificationItem> Notifications => Set<NotificationItem>();
        public DbSet<LabRule> LabRules => Set<LabRule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasKey(s => s.IdNumber);

            modelBuilder.Entity<Reservation>()
                .HasOne(reservation => reservation.Student)
                .WithMany()
                .HasForeignKey(reservation => reservation.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SitInSession>()
                .HasOne(session => session.Student)
                .WithMany()
                .HasForeignKey(session => session.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SitInSession>()
                .HasOne(session => session.Reservation)
                .WithMany()
                .HasForeignKey(session => session.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Feedback>()
                .HasOne(item => item.SitInSession)
                .WithMany()
                .HasForeignKey(item => item.SitInSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Feedback>()
                .HasOne(item => item.Student)
                .WithMany()
                .HasForeignKey(item => item.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RewardPoint>()
                .HasOne(item => item.Student)
                .WithMany()
                .HasForeignKey(item => item.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RewardPoint>()
                .HasOne(item => item.SitInSession)
                .WithMany()
                .HasForeignKey(item => item.SitInSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RewardPoint>()
                .HasIndex(item => item.SitInSessionId)
                .IsUnique()
                .HasFilter("\"SitInSessionId\" IS NOT NULL");

            modelBuilder.Entity<NotificationItem>()
                .HasOne(item => item.Student)
                .WithMany()
                .HasForeignKey(item => item.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LabRule>().HasData(
                new LabRule { Id = 1, Title = "Use assigned workstations only", Description = "Students must use the computer unit assigned during reservation or sit-in approval.", DisplayOrder = 1 },
                new LabRule { Id = 2, Title = "Keep sessions academic", Description = "Laboratory sessions are for programming, research, coursework, and approved CCS activities.", DisplayOrder = 2 },
                new LabRule { Id = 3, Title = "Report issues before leaving", Description = "Hardware, software, or network problems should be reported to the laboratory staff before checkout.", DisplayOrder = 3 }
            );
        }
    }
}
