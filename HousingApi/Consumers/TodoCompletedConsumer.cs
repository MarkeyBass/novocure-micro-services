using System.Text;
using System.Text.Json;
using HousingApi.Messages;
using HousingApi.Models;
using HousingApi.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HousingApi.Consumers;

public class TodoCompletedConsumer : BackgroundService
{
    private const string ExchangeName = "todo.completed";
    private const string QueueName = "housing-api.todo-completed";

    private readonly HousingApplicationsService _applicationsService;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<TodoCompletedConsumer> _logger;

    public TodoCompletedConsumer(
        HousingApplicationsService applicationsService,
        IOptions<RabbitMqSettings> options,
        ILogger<TodoCompletedConsumer> logger
    )
    {
        _applicationsService = applicationsService;
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Task.Run so the blocking consumer loop does not stall app startup.
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

        // Durable queue: survives RabbitMQ restart and holds events until consumed.
        channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind queue to exchange — fanout ignores routing key.
        channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: "");

        // One message at a time — each involves a Mongo update.
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
            _logger.LogInformation("Received TodoCompleted message");

            try
            {
                var message = JsonSerializer.Deserialize<TodoCompleted>(
                    body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (message is null)
                {
                    _logger.LogWarning("Deserialized TodoCompleted was null — discarding");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                // Guard: HousingApplicationId must be present.
                if (string.IsNullOrWhiteSpace(message.HousingApplicationId))
                {
                    _logger.LogInformation(
                        "TodoCompleted for TodoId={TodoId} has no HousingApplicationId — acking without update",
                        message.TodoId
                    );
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                // Verify the application exists before updating.
                // GetAsync + IncrementCompletedReviewAsync are both sync-over-async here;
                // EventingBasicConsumer callbacks are sync — acceptable for this stage.
                var application = _applicationsService
                    .GetAsync(message.HousingApplicationId)
                    .GetAwaiter()
                    .GetResult();

                if (application is null)
                {
                    _logger.LogInformation(
                        "TodoCompleted for TodoId={TodoId} references unknown HousingApplicationId={HousingApplicationId} — acking without update",
                        message.TodoId,
                        message.HousingApplicationId
                    );
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                _applicationsService
                    .IncrementCompletedReviewAsync(
                        message.HousingApplicationId,
                        message.CompletedAt
                    )
                    .GetAwaiter()
                    .GetResult();

                _logger.LogInformation(
                    "Incremented CompletedReviewCount for HousingApplicationId={HousingApplicationId} (TodoId={TodoId})",
                    message.HousingApplicationId,
                    message.TodoId
                );

                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process TodoCompleted — message will be requeued");
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        // Block until app shuts down.
        stoppingToken.WaitHandle.WaitOne();

        _logger.LogInformation("TodoCompletedConsumer stopping");
    }
}
