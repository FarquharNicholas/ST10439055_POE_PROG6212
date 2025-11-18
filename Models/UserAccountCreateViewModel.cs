using System.ComponentModel.DataAnnotations;

namespace ST10439055_POE_PROG6212.Models
{
    public class UserAccountCreateViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Department { get; set; } = string.Empty;

        [Range(0, 2000)]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Lecturer;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Temporary Password")]
        public string Password { get; set; } = string.Empty;
    }
}

