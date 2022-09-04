using MessageFlow.Workflow;

namespace MessageFlow.Entities;

/// <summary>
/// default definition for a worker with no output and config
/// </summary>
public abstract class DefaultDefinition<TInput> :
	DefaultDefinition<TInput, EndOfWorkflow, EmptyConfig> { }

/// <summary>
/// default definition for a worker with no config
/// </summary>
public abstract class DefaultDefinition<TInput, TOutput> :
	DefaultDefinition<TInput, TOutput, EmptyConfig> { }

/// <summary>
/// default definition for a worker
/// </summary>
public abstract class DefaultDefinition<TInput, TOutput, TConfig> :
	IWorkerDefinition<TInput, TOutput, TConfig> {

	/// <summary>
	/// default config to pass to the worker when starting the workflow; can be 
	/// overridden when defining a workflow
	/// </summary>
	public TConfig? Config { get; set; }

	/// <summary>
	/// serialized version of <see cref="Config" /> as it is written to the message
	/// </summary>
	public string? SerializedConfig { get; set; }

	/// <summary>
	/// project this worker belongs to; will be part of exchange and queue name
	/// </summary>
	public virtual string Project { get; } = "Main";

	/// <summary>
	/// name of the action the worker is performing; will be part of queue name and
	/// rouding key; default will be derived from class name
	/// </summary>
	public virtual string Action => GetActionName();

	/// <summary>
	/// further specifies the action performed by the worker; will be part of queue
	/// name and routing key
	/// </summary>
	public virtual string? ActionVariant { get; } = null;

	/// <summary>
	/// name of the input type; will be part of the queue name and routing key
	/// </summary>
	public string InputTypeName => typeof(TInput).Name;

	public virtual string Exchange => Project;
	public virtual string QueueName => GetQueueName();
	public virtual string RoutingKey => GetRoutingKey();
	public virtual int PrefetchCount => 20;

	private string GetQueueName() {
		if (Project is null) return GetRoutingKey();
		return $"{Project}.{GetRoutingKey()}";
	}

	private string GetRoutingKey() {
		var key = $"{InputTypeName}.{Action}";
		if (ActionVariant is null) return key;
		return $"{key}.{ActionVariant}";
	}

	private string GetActionName() {
		return GetType().Name.Replace("Definition", "");
	}
}
