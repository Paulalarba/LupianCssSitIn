using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class NotificationItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(400)]
        public string Message { get; set; } = string.Empty;

        [StringLength(30)]
        public string Type { get; set; } = "Info";

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
