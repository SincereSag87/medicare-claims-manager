using System.ComponentModel.DataAnnotations;

namespace medicare_claims_manager.Models;

public class Patient
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(32)]
    [Display(Name = "Medicare Number")]
    public string MedicareNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateOnly DateOfBirth { get; set; }

    [EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(32)]
    public string? Phone { get; set; }

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
