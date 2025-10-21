namespace ST10439055_POE_PROG6212.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
