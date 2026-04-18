namespace TodoApi.Messages;

// Mirror of HousingApi.Messages.HousingApplicationCreated.
// Both sides define the contract independently — no shared library.
// Field names and types must stay in sync with the producer.
// Message created by HousingApi.Services.RabbitMqPublisher.Publish()
// Message consumed by TodoApi.Consumers.HousingApplicationCreatedConsumer.Consume()
public record HousingApplicationCreated(
    string ApplicationId,
    string HousingId,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt
);
