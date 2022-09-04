namespace MessageFlow.Entities;

/// <summary>
/// RabbitMQ connector configuration
/// </summary>
public class RabbitMqConfig {
	public string Hostname { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string Username { get; set; } = "rabbit";
	public string Passwort { get; set; } = "pass";

	/// <summary>
	/// environment to create the queue setup in; should be something like "prod",
	/// "staging", "dev", ...
	/// </summary>
	public string Environment { get; set; } = "prod";

	/// <summary>
	/// How to distinguish different environments.
	/// </summary>
	public EnvironmentMode EnvironmentMode { get; set; } = EnvironmentMode.NamePrefix;
}
