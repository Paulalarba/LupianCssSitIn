using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class Student
    {
        public const int DefaultSessionAllocation = 30;

        [Key]
        [Required]
        [StringLength(40)]
        public string IdNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string MiddleName { get; set; } = string.Empty;

        [StringLength(40)]
        public string CourseLevel { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(80)]
        public string Course { get; set; } = string.Empty;

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        public string ProfilePictureUrl { get; set; } = string.Empty;

        [StringLength(100)]
        public string EmergencyContactName { get; set; } = "Not set";

        [StringLength(30)]
        public string EmergencyContactNumber { get; set; } = "Not set";

        public int TotalSessions { get; set; } = DefaultSessionAllocation;

        public int RemainingSessions { get; set; } = DefaultSessionAllocation;

        public int TotalSessionsUsed { get; set; }

        public int Points { get; set; }

        public int RewardPoints { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        [NotMapped]
        public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
