namespace ST10439055_POE_PROG6212.Models
{
    public enum ClaimVerificationStatus
    {
        Pass,
        Warning,
        Fail
    }

    public class ClaimVerificationResult
    {
        public string RuleName { get; set; } = string.Empty;
        public ClaimVerificationStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

