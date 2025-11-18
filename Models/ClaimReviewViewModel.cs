using System.Linq;

namespace ST10439055_POE_PROG6212.Models
{
    public class ClaimReviewViewModel
    {
        public Claim Claim { get; set; } = null!;
        public IEnumerable<ClaimVerificationResult> VerificationResults { get; set; } = Enumerable.Empty<ClaimVerificationResult>();

        public bool HasBlockingIssues => VerificationResults.Any(r => r.Status == ClaimVerificationStatus.Fail);
        public bool RequiresAttention => VerificationResults.Any(r => r.Status != ClaimVerificationStatus.Pass);
    }
}

