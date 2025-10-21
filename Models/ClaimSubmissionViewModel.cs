using System.ComponentModel.DataAnnotations;

namespace ST10439055_POE_PROG6212.Models
{
    public class ClaimSubmissionViewModel
    {
        [Required(ErrorMessage = "Lecturer name is required")]
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [DataType(DataType.Date)]
        public DateTime Month { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be between 0.01 and 1000")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile? SupportingDocument { get; set; }
    }
}
