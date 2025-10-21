public class Claim
{
    public int ClaimId { get; set; }
    public int LecturerId { get; set; }
    public DateTime Month { get; set; }
    public int HoursWorked { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public Lecturer Lecturer { get; set; }
    public ICollection<SupportingDocument> SupportingDocuments { get; set; }
    public ICollection<Approval> Approvals { get; set; }
}
