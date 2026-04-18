namespace TodoApi.Models;

public class UpdateTodoItemDTO
{
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? HousingApplicationId { get; set; }
}
