using System.Text;
using System.Text.Json;
using HousingApi.Messages;
using HousingApi.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace HousingApi.Services;

// Registered as a singleton — one connection shared for the lifetime of the app.
// IDisposable so ASP.NET Core disposes the connection cleanly on shutdown.
public class RabbitMqPublisher : IDisposable
{
    // Fanout exchange: every bound queue receives every message regardless of routing key.
    // The consumer (TodoApi) will declare its own queue and bind it to this exchange.
    private const string ExchangeName = "housing.application.created";

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> options, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = options.Value.HostName,
            UserName = options.Value.UserName,
            Password = options.Value.Password,
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // durable: true — exchange survives a RabbitMQ restart
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);

        _logger.LogInformation(
            "RabbitMQ connected to {HostName}, exchange '{Exchange}' ready",
            options.Value.HostName,
            ExchangeName
        );
    }

    public void Publish(HousingApplicationCreated message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true; // message survives broker restart
        props.ContentType = "application/json";

        // Fanout exchange ignores the routing key — pass empty string by convention.
        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "",
            basicProperties: props,
            body: body
        );

        _logger.LogInformation(
            "Published HousingApplicationCreated: ApplicationId={ApplicationId}, HousingId={HousingId}",
            message.ApplicationId,
            message.HousingId
        );
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
