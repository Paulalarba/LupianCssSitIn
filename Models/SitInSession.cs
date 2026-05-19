using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class SitInSession
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        public int? ReservationId { get; set; }

        public Reservation? Reservation { get; set; }

        [Required]
        [StringLength(80)]
        public string LabName { get; set; } = string.Empty;

        [Required]
        [StringLength(160)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LanguageUsed { get; set; } = "Not Specified";

        public DateTime TimeIn { get; set; } = DateTime.UtcNow;

        public DateTime? TimeOut { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        [StringLength(20)]
        public string? SeatNumber { get; set; }

        public string? ApprovedByAdminId { get; set; }

        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

        public int PointsAwarded { get; set; }

        public bool IsRewardEvaluated { get; set; }

        public bool HasViolation { get; set; }

        [StringLength(300)]
        public string? ViolationRemarks { get; set; }

        public DateTime? RewardEvaluatedAt { get; set; }

        public string? Notes { get; set; }

        [NotMapped]
        public double DurationHours => ((TimeOut ?? DateTime.UtcNow) - TimeIn).TotalHours;
    }
}
