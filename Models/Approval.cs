namespace ST10439055_POE_PROG6212.Models
{
    public class Approval
    {
        public int ApprovalId { get; set; }
        public int ClaimId { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ApprovalDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public Claim Claim { get; set; } = null!;
    }
}
