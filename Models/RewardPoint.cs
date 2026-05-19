using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class RewardPoint
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        public int? SitInSessionId { get; set; }

        public SitInSession? SitInSession { get; set; }

        public int Points { get; set; }

        [Required]
        [StringLength(120)]
        public string Source { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Notes { get; set; }

        public bool IsManual { get; set; }

        public string? AwardedByAdminId { get; set; }

        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    }
}
