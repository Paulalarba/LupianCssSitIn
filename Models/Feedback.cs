using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public int SitInSessionId { get; set; }

        public SitInSession? SitInSession { get; set; }

        [Required]
        [StringLength(40)]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        public string? AdminId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required]
        [StringLength(600)]
        public string Comments { get; set; } = string.Empty;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
