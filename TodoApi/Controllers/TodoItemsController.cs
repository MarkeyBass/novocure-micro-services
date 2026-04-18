using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Messages;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodoItemsController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<TodoItemsController> _logger;

    public TodoItemsController(
        TodoContext context,
        RabbitMqPublisher publisher,
        ILogger<TodoItemsController> logger
    )
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    // GET: api/TodoItems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
    {
        return await _context.TodoItems.Select(x => ItemToDTO(x)).ToListAsync();
    }

    // GET: api/TodoItems/5
    // <snippet_GetByID>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return ItemToDTO(todoItem);
    }

    // </snippet_GetByID>

    // PUT: api/TodoItems/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Update>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItemDTO todoDTO)
    {
        // Allow clients (especially Swagger/manual tests) to identify the resource by the
        // route id only. If the body includes a non-default id, it must still match.
        if (todoDTO.Id != default && id != todoDTO.Id)
        {
            return BadRequest();
        }

        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        // Capture the pre-update state so we can detect the false → true transition.
        var wasComplete = todoItem.IsComplete;

        var now = DateTime.UtcNow;
        // Preserve the existing Name when the client omits it or sends an empty string.
        if (!string.IsNullOrWhiteSpace(todoDTO.Name))
        {
            todoItem.Name = todoDTO.Name;
        }
        todoItem.IsComplete = todoDTO.IsComplete;

        // Preserve the existing HousingApplicationId when the client omits it or sends
        // an empty string from Swagger. This keeps the todo linked to its housing
        // application when a caller only toggles completion status.
        if (!string.IsNullOrWhiteSpace(todoDTO.HousingApplicationId))
        {
            todoItem.HousingApplicationId = NormalizeHousingApplicationId(
                todoDTO.HousingApplicationId
            );
        }

        todoItem.UpdatedAt = now;
        todoItem.CompletedAt = todoDTO.IsComplete ? todoItem.CompletedAt ?? now : null;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TodoItemExists(id))
        {
            return NotFound();
        }

        // Only publish when the todo transitions from incomplete to complete.
        // No event for: create, false→false, true→true, true→false.
        if (!wasComplete && todoItem.IsComplete)
        {
            var housingAppId = todoItem.HousingApplicationId;
            if (!string.IsNullOrWhiteSpace(housingAppId))
            {
                _publisher.Publish(
                    new TodoCompleted(
                        TodoId: todoItem.Id,
                        HousingApplicationId: housingAppId,
                        CompletedAt: todoItem.CompletedAt ?? now,
                        Name: todoItem.Name
                    )
                );
            }
            else
            {
                _logger.LogInformation(
                    "Todo {TodoId} completed but has no HousingApplicationId — skipping publish",
                    todoItem.Id
                );
            }
        }
        else
        {
            _logger.LogInformation(
                "Todo {TodoId} updated (wasComplete={WasComplete}, isComplete={IsComplete}) — no completion transition, skipping publish",
                todoItem.Id,
                wasComplete,
                todoItem.IsComplete
            );
        }

        return NoContent();
    }

    // </snippet_Update>

    // POST: api/TodoItems
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    // <snippet_Create>
    [HttpPost]
    public async Task<ActionResult<TodoItemDTO>> PostTodoItem(CreateTodoItemDTO todoDTO)
    {
        var now = DateTime.UtcNow;
        var todoItem = new TodoItem
        {
            IsComplete = todoDTO.IsComplete,
            Name = todoDTO.Name,
            HousingApplicationId = NormalizeHousingApplicationId(todoDTO.HousingApplicationId),
            CreatedAt = now,
            UpdatedAt = now,
            CompletedAt = todoDTO.IsComplete ? now : null,
        };

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, ItemToDTO(todoItem));
    }

    // </snippet_Create>

    // DELETE: api/TodoItems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TodoItemExists(long id)
    {
        return _context.TodoItems.Any(e => e.Id == id);
    }

    private static string? NormalizeHousingApplicationId(string? housingApplicationId) =>
        string.IsNullOrWhiteSpace(housingApplicationId) ? null : housingApplicationId;

    private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
        new TodoItemDTO
        {
            Id = todoItem.Id,
            Name = todoItem.Name,
            IsComplete = todoItem.IsComplete,
            HousingApplicationId = todoItem.HousingApplicationId ?? string.Empty,
            CreatedAt = todoItem.CreatedAt,
            UpdatedAt = todoItem.UpdatedAt,
            CompletedAt = todoItem.CompletedAt,
        };
}
