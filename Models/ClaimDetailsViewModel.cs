namespace medicare_claims_manager.Models;

public class ClaimDetailsViewModel
{
    public Claim Claim { get; set; } = new();

    public IReadOnlyList<ClaimStatus> NextStatuses { get; set; } = Array.Empty<ClaimStatus>();
}
