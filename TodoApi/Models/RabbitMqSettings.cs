namespace TodoApi.Models;

public class RabbitMqSettings
{
    // Defaults to localhost for host development.
    // Override with RabbitMQ__HostName=rabbitmq when running in Docker Compose.
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "admin";
    public string Password { get; set; } = "admin";
}
