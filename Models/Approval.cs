public class Approval
{
    public int ApprovalId { get; set; }
    public int ClaimId { get; set; }
    public string ApprovedBy { get; set; }
    public DateTime ApprovalDate { get; set; }
    public string Remarks { get; set; }
    public Claim Claim { get; set; }
}
