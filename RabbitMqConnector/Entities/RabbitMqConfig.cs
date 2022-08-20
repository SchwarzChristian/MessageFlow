namespace RabbitMqConnector.Entities;

public class RabbitMqConfig {
	public string Hostname { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string Username { get; set; } = "rabbit";
	public string Passwort { get; set; } = "pass";
	public string Environment { get; set; } = "prod";
}