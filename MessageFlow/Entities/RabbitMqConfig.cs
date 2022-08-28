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
	/// Environment to create the queue setup in.
	/// </summary>
	public string Environment { get; set; } = "prod";

	/// <summary>
	/// How to distinguish different environments.
	/// </summary>
	public EnvironmentMode EnvironmentMode { get; set; } = EnvironmentMode.NamePrefix;
}

public enum EnvironmentMode {
	/// <summary>
	/// Create exchanges and queues in the virtual host with the environment name.
	/// </summary>
	VHost,

	/// <summary>
	/// Create exchanges and queues in the default virtual host, but add the environment
	/// name to exchange names, queue names and routing keys.
	/// </summary>
	NamePrefix,
}