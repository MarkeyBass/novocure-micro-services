namespace TodoApi.Models;

public class TodoItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
    // public string? HousingApplicationId { get; set; }
    // public DateTime? CreatedAt { get; set; }
    // public DateTime? UpdatedAt { get; set; }
    // public DateTime? CompletedAt { get; set; }
}
