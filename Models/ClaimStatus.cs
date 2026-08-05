namespace medicare_claims_manager.Models;

public enum ClaimStatus
{
    Draft,
    Submitted,
    InReview,
    PendingDocumentation,
    Approved,
    Denied,
    Paid
}
