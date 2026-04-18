namespace TodoApi.Messages;

// Published when a TodoItem transitions from IsComplete=false to IsComplete=true
// and has a non-empty HousingApplicationId.
// Message created by TodoApi.Services.RabbitMqPublisher.Publish()
// Message consumed by HousingApi.Consumers.TodoCompletedConsumer.Consume()
public record TodoCompleted(
    long TodoId,
    string HousingApplicationId,
    DateTime CompletedAt,
    string? Name
);
