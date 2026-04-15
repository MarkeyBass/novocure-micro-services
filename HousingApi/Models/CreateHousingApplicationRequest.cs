using System.ComponentModel.DataAnnotations;

namespace HousingApi.Models;

public class CreateHousingApplicationRequest
{
    [Required]
    public string HousingId { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
