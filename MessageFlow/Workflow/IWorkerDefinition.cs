namespace MessageFlow.Workflow;

/// <summary>
/// type-safe worker definition
/// </summary>
public interface IWorkerDefinition<TInput, TOutput, TConfig> : IWorkerDefinition {
	/// <summary>
	/// default config to pass to the worker when starting the workflow; can be 
	/// overridden when defining a workflow
	/// </summary>
	TConfig? Config { get; set; }
}

/// <summary>
/// untyped worker definition
/// </summary>
public interface IWorkerDefinition {
	/// <summary>
	/// project this worker belongs to; will be part of exchange and queue name
	/// </summary>
	public string? Project { get; }

	/// <summary>
	/// name of the action the worker is performing; will be part of queue name and
	/// rouding key; default will be derived from class name
	/// </summary>
	string Action { get; }

	/// <summary>
	/// further specifies the action performed by the worker; will be part of queue
	/// name and routing key
	/// </summary>
	string? ActionVariant { get; }

	/// <summary>
	/// name of the input type; will be part of the queue name and routing key
	/// </summary>
	string InputTypeName { get; }
	string Exchange { get; }
	string QueueName { get; }
	string RoutingKey { get; }

	/// <summary>
	/// serialized config as it is written to the message
	/// </summary>
	string? SerializedConfig { get; set; }
}