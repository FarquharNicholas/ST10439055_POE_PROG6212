using System.ComponentModel.DataAnnotations;

namespace ST10439055_POE_PROG6212.Models
{
    public class ClaimSubmissionViewModel
    {
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [DataType(DataType.Date)]
        public DateTime Month { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 180, ErrorMessage = "Hours worked must be between 1 and 180")]
        [Display(Name = "Hours Worked")]
        public int HoursWorked { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile? SupportingDocument { get; set; }
    }
}
