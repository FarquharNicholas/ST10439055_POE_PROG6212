namespace ST10439055_POE_PROG6212.Models
{
    public class SupportingDocument
    {
        public int SupportingDocumentId { get; set; }
        public int ClaimId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public Claim Claim { get; set; } = null!;
    }
}
