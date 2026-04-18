namespace HousingApi.Messages;

// Mirror of TodoApi.Messages.TodoCompleted.
// Both sides define the contract independently — no shared library.
// Field names and types must stay in sync with the producer.
public record TodoCompleted(
    long TodoId,
    string HousingApplicationId,
    DateTime CompletedAt,
    string? Name
);
