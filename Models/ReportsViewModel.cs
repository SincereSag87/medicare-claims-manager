namespace medicare_claims_manager.Models;

public class ReportsViewModel
{
    public int TotalClaims { get; set; }

    public int OpenClaims { get; set; }

    public int PaidClaims { get; set; }

    public int DeniedClaims { get; set; }

    public decimal TotalBilled { get; set; }

    public decimal TotalApproved { get; set; }

    public decimal TotalOutstanding { get; set; }

    public decimal ApprovalRate { get; set; }

    public IReadOnlyList<StatusReportItem> StatusBreakdown { get; set; } = Array.Empty<StatusReportItem>();

    public IReadOnlyList<PriorityReportItem> PriorityBreakdown { get; set; } = Array.Empty<PriorityReportItem>();

    public IReadOnlyList<ProviderReportItem> ProviderPerformance { get; set; } = Array.Empty<ProviderReportItem>();

    public IReadOnlyList<ClaimAuditEntry> RecentWorkflowActivity { get; set; } = Array.Empty<ClaimAuditEntry>();
}

public class StatusReportItem
{
    public ClaimStatus Status { get; set; }

    public int Count { get; set; }

    public decimal BilledAmount { get; set; }
}

public class PriorityReportItem
{
    public ClaimPriority Priority { get; set; }

    public int Count { get; set; }
}

public class ProviderReportItem
{
    public string ProviderName { get; set; } = string.Empty;

    public int ClaimCount { get; set; }

    public decimal BilledAmount { get; set; }

    public decimal ApprovedAmount { get; set; }
}
