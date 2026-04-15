namespace HousingApi.Models;

public class HousingApplicationResponse
{
    public string Id { get; set; } = null!;

    public string HousingId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
