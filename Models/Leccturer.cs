using System.Security.Claims;

public class Lecturer
{
    public int LecturerId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public decimal HourlyRate { get; set; }
    public ICollection<Claim> Claims { get; set; }
}
