using System.ComponentModel.DataAnnotations;

namespace CCSMonitoringSystem.Models
{
    public class LabRule
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
