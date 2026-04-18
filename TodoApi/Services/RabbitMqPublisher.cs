using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TodoApi.Messages;
using TodoApi.Models;

namespace TodoApi.Services;

// Singleton publisher — one connection + channel for the app lifetime.
// IDisposable so ASP.NET Core closes the connection cleanly on shutdown.
public class RabbitMqPublisher : IDisposable
{
    private const string ExchangeName = "todo.completed";

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

        // durable: true — exchange survives a RabbitMQ restart.
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);

        _logger.LogInformation(
            "RabbitMQ publisher connected to {HostName}, exchange '{Exchange}' ready",
            options.Value.HostName,
            ExchangeName
        );
    }

    public void Publish(TodoCompleted message)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        // Fanout exchange ignores the routing key — empty string by convention.
        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "",
            basicProperties: props,
            body: body
        );

        _logger.LogInformation(
            "Published TodoCompleted: TodoId={TodoId}, HousingApplicationId={HousingApplicationId}",
            message.TodoId,
            message.HousingApplicationId
        );
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
