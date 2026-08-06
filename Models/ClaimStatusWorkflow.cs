namespace medicare_claims_manager.Models;

public static class ClaimStatusWorkflow
{
    private static readonly IReadOnlyDictionary<ClaimStatus, ClaimStatus[]> Transitions = new Dictionary<ClaimStatus, ClaimStatus[]>
    {
        [ClaimStatus.Draft] = [ClaimStatus.Submitted],
        [ClaimStatus.Submitted] = [ClaimStatus.InReview, ClaimStatus.PendingDocumentation],
        [ClaimStatus.InReview] = [ClaimStatus.PendingDocumentation, ClaimStatus.Approved, ClaimStatus.Denied],
        [ClaimStatus.PendingDocumentation] = [ClaimStatus.InReview, ClaimStatus.Denied],
        [ClaimStatus.Approved] = [ClaimStatus.Paid],
        [ClaimStatus.Denied] = [],
        [ClaimStatus.Paid] = []
    };

    public static IReadOnlyList<ClaimStatus> GetNextStatuses(ClaimStatus currentStatus)
    {
        return Transitions.TryGetValue(currentStatus, out var statuses) ? statuses : [];
    }

    public static bool CanTransition(ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        return GetNextStatuses(currentStatus).Contains(nextStatus);
    }
}
