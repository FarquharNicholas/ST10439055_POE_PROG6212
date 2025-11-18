using System.ComponentModel.DataAnnotations;

namespace ST10439055_POE_PROG6212.Models
{
    public class LecturerEditViewModel
    {
        [Required]
        public int LecturerId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(0, 2000)]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Lecturer;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}


