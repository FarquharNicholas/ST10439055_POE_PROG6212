using System.ComponentModel.DataAnnotations;

namespace ST10439055_POE_PROG6212.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}

