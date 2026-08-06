using System.ComponentModel.DataAnnotations;

namespace medicare_claims_manager.Models;

public class Provider
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    [Display(Name = "Organization Name")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "NPI must be exactly 10 digits.")]
    [Display(Name = "NPI")]
    public string Npi { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Specialty { get; set; } = string.Empty;

    [Required, EmailAddress]
    [StringLength(160)]
    [Display(Name = "Contact Email")]
    public string ContactEmail { get; set; } = string.Empty;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
