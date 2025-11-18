using System.Linq;

namespace ST10439055_POE_PROG6212.Models
{
    public class HRDashboardViewModel
    {
        public IEnumerable<Lecturer> Lecturers { get; set; } = Enumerable.Empty<Lecturer>();
        public IEnumerable<ApprovedClaimReportRow> ApprovedClaims { get; set; } = Enumerable.Empty<ApprovedClaimReportRow>();
        public decimal TotalApprovedAmount => ApprovedClaims.Sum(c => c.TotalAmount);
        public int TotalApprovedHours => ApprovedClaims.Sum(c => c.HoursWorked);
        public int LecturerCount => Lecturers.Count();
    }
}

