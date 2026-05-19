using System.ComponentModel.DataAnnotations;

namespace CCSMonitoringSystem.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string Audience { get; set; } = "All";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? AdminId { get; set; }

        public string PostedBy { get; set; } = "CCS Admin";
    }
}
