using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicare_claims_manager.Models;

public class Claim
{
    public int Id { get; set; }

    [Required, StringLength(32)]
    public string ClaimNumber { get; set; } = string.Empty;

    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    public int ProviderId { get; set; }

    public Provider? Provider { get; set; }

    [DataType(DataType.Date)]
    public DateOnly ServiceDate { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal BilledAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? ApprovedAmount { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

    public ClaimPriority Priority { get; set; } = ClaimPriority.Standard;

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
