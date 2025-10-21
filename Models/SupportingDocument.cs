public class SupportingDocument
{
    public int SupportingDocumentId { get; set; }
    public int ClaimId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadedAt { get; set; }
    public Claim Claim { get; set; }
}
