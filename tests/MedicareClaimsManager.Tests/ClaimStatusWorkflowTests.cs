using medicare_claims_manager.Models;

namespace MedicareClaimsManager.Tests;

public class ClaimStatusWorkflowTests
{
    [Theory]
    [InlineData(ClaimStatus.Draft, ClaimStatus.Submitted)]
    [InlineData(ClaimStatus.Submitted, ClaimStatus.InReview)]
    [InlineData(ClaimStatus.Submitted, ClaimStatus.PendingDocumentation)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.PendingDocumentation)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.Approved)]
    [InlineData(ClaimStatus.InReview, ClaimStatus.Denied)]
    [InlineData(ClaimStatus.PendingDocumentation, ClaimStatus.InReview)]
    [InlineData(ClaimStatus.PendingDocumentation, ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Approved, ClaimStatus.Paid)]
    public void CanTransition_ReturnsTrue_ForAllowedTransition(ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        var canTransition = ClaimStatusWorkflow.CanTransition(currentStatus, nextStatus);

        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(ClaimStatus.Draft, ClaimStatus.Paid)]
    [InlineData(ClaimStatus.Submitted, ClaimStatus.Paid)]
    [InlineData(ClaimStatus.PendingDocumentation, ClaimStatus.Approved)]
    [InlineData(ClaimStatus.Approved, ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Denied, ClaimStatus.InReview)]
    [InlineData(ClaimStatus.Paid, ClaimStatus.InReview)]
    public void CanTransition_ReturnsFalse_ForInvalidTransition(ClaimStatus currentStatus, ClaimStatus nextStatus)
    {
        var canTransition = ClaimStatusWorkflow.CanTransition(currentStatus, nextStatus);

        Assert.False(canTransition);
    }

    [Theory]
    [InlineData(ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Paid)]
    public void GetNextStatuses_ReturnsEmpty_ForTerminalStatus(ClaimStatus status)
    {
        var nextStatuses = ClaimStatusWorkflow.GetNextStatuses(status);

        Assert.Empty(nextStatuses);
    }
}
