using System.ComponentModel.DataAnnotations;

namespace medicare_claims_manager.Models;

public class ClaimAuditEntry
{
    public int Id { get; set; }

    public int ClaimId { get; set; }

    public Claim? Claim { get; set; }

    [Required, StringLength(80)]
    public string Action { get; set; } = string.Empty;

    [StringLength(80)]
    public string? FieldName { get; set; }

    [StringLength(500)]
    public string? OldValue { get; set; }

    [StringLength(500)]
    public string? NewValue { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required, StringLength(256)]
    public string ChangedBy { get; set; } = string.Empty;

    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
}
