namespace ST10439055_POE_PROG6212.Models
{
    public class ApprovedClaimReportRow
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }

        public string MonthDisplay => Month.ToString("MMMM yyyy");
    }
}


