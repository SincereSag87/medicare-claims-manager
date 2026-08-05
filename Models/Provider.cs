using System.ComponentModel.DataAnnotations;

namespace medicare_claims_manager.Models;

public class Provider
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Npi { get; set; } = string.Empty;

    [StringLength(120)]
    public string Specialty { get; set; } = string.Empty;

    [StringLength(160)]
    public string ContactEmail { get; set; } = string.Empty;

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
