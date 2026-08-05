using System.ComponentModel.DataAnnotations;

namespace medicare_claims_manager.Models;

public class Patient
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(32)]
    public string MedicareNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(32)]
    public string? Phone { get; set; }

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
