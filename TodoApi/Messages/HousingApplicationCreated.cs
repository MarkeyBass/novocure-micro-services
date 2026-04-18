namespace TodoApi.Messages;

// Mirror of HousingApi.Messages.HousingApplicationCreated.
// Both sides define the contract independently — no shared library.
// Field names and types must stay in sync with the producer.
public record HousingApplicationCreated(
    string ApplicationId,
    string HousingId,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt
);
