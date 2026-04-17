namespace HousingApi.Messages;

// Event published to RabbitMQ after a housing application is successfully persisted.
// Keep all fields that a consumer (e.g. TodoApi) needs to act without querying HousingApi.
public record HousingApplicationCreated(
    string ApplicationId,
    string HousingId,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt
);
