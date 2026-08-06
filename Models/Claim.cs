using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicare_claims_manager.Models;

public class Claim
{
    public int Id { get; set; }

    [Required, StringLength(32)]
    [Display(Name = "Claim Number")]
    public string ClaimNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    public Patient? Patient { get; set; }

    [Required]
    [Display(Name = "Provider")]
    public int ProviderId { get; set; }

    public Provider? Provider { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Service Date")]
    public DateOnly ServiceDate { get; set; }

    [Range(0.01, 9999999999.99)]
    [Column(TypeName = "decimal(12,2)")]
    [DataType(DataType.Currency)]
    [Display(Name = "Billed Amount")]
    public decimal BilledAmount { get; set; }

    [Range(0, 9999999999.99)]
    [Column(TypeName = "decimal(12,2)")]
    [DataType(DataType.Currency)]
    [Display(Name = "Approved Amount")]
    public decimal? ApprovedAmount { get; set; }

    [Required]
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

    [Required]
    public ClaimPriority Priority { get; set; } = ClaimPriority.Standard;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Display(Name = "Created")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Display(Name = "Updated")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
