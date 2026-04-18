using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TodoApi.Messages;
using TodoApi.Models;

namespace TodoApi.Consumers;

// BackgroundService is ASP.NET Core's base class for long-running hosted services.
// It implements IHostedService and runs ExecuteAsync on app startup.
public class HousingApplicationCreatedConsumer : BackgroundService
{
    private const string ExchangeName = "housing.application.created";

    // Queue name follows the convention: {consumer-service}.{exchange-name}
    // This makes it clear which service owns the queue and what event it handles.
    private const string QueueName = "todo-api.housing-application-created";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<HousingApplicationCreatedConsumer> _logger;

    public HousingApplicationCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> options,
        ILogger<HousingApplicationCreatedConsumer> logger
    )
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Task.Run moves the blocking consumer loop off the startup thread.
        // Without this, ExecuteAsync would block app startup until the consumer exits.
        return Task.Run(() => RunConsumerLoop(stoppingToken), stoppingToken);
    }

    private void RunConsumerLoop(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password,
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declare the fanout exchange — idempotent, matches producer declaration.
        channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout, durable: true);

        // Declare a durable queue owned by this consumer.
        // durable: true — queue survives a RabbitMQ restart and will hold messages until consumed.
        // exclusive: false — other consumers could bind (useful for horizontal scaling later).
        // autoDelete: false — queue stays alive even when no consumers are connected.
        channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind the queue to the exchange.
        // Fanout ignores the routing key — empty string is the convention.
        channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: "");

        // prefetchCount: 1 — process one message at a time before sending the next.
        // Prevents a backlog of messages being pushed to this consumer simultaneously.
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation(
            "RabbitMQ consumer ready — queue '{Queue}' bound to exchange '{Exchange}'",
            QueueName,
            ExchangeName
        );

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received HousingApplicationCreated message");

            try
            {
                var message = JsonSerializer.Deserialize<HousingApplicationCreated>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (message is null)
                {
                    _logger.LogWarning("Deserialized message was null — discarding");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                // TodoContext is scoped (AddDbContext default lifetime).
                // BackgroundService is effectively singleton, so we must create a scope
                // for each message to get a fresh DbContext instead of sharing one across messages.
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
                var now = DateTime.UtcNow;

                var todo = new TodoItem
                {
                    Name = $"Review housing application for {message.FirstName} {message.LastName}",
                    HousingApplicationId = message.ApplicationId,
                    IsComplete = false,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                db.TodoItems.Add(todo);
                db.SaveChanges();

                _logger.LogInformation(
                    "Created todo (Id={TodoId}) for ApplicationId={ApplicationId}",
                    todo.Id,
                    message.ApplicationId
                );

                // Acknowledge after successful DB write — message is removed from the queue.
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process HousingApplicationCreated — message will be requeued"
                );

                // Nack with requeue: true returns the message to the queue for retry.
                // Without a DLQ this can cause infinite retry loops on poison messages,
                // but is acceptable for this stage.
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        // autoAck: false — we manually ack/nack so messages are not lost on processing failure.
        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        // Block this thread until the app shuts down.
        stoppingToken.WaitHandle.WaitOne();

        _logger.LogInformation("HousingApplicationCreatedConsumer stopping");
    }
}
