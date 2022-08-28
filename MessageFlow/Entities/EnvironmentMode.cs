namespace MessageFlow.Entities;

/// <summary>
/// how to distinguish between different environments
/// </summary>
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