namespace medicare_claims_manager.Models;

public class DashboardViewModel
{
    public int TotalPatients { get; set; }

    public int TotalProviders { get; set; }

    public int OpenClaims { get; set; }

    public decimal PendingClaimValue { get; set; }

    public IReadOnlyList<Claim> RecentClaims { get; set; } = Array.Empty<Claim>();
}
