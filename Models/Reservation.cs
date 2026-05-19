using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        [Required]
        [StringLength(80)]
        public string LabName { get; set; } = string.Empty;

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(160)]
        public string Purpose { get; set; } = string.Empty;

        [StringLength(400)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(20)]
        public string SeatNumber { get; set; } = string.Empty;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public string? ReviewedByAdminId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(300)]
        public string? ReviewRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? SitInSessionId { get; set; }
    }
}
