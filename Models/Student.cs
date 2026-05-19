using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCSMonitoringSystem.Models
{
    public class Student
    {
        [Key]
        [Required]
        public string IdNumber { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string CourseLevel { get; set; }

        public bool RememberMe { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Course { get; set; }

        public string Address { get; set; }

        public string ProfilePictureUrl { get; set; }

        [NotMapped]
        public IFormFile ProfilePictureFile { get; set; }
    }
}
