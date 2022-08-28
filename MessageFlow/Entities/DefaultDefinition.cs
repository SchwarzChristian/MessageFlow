using MessageFlow.Workflow;

namespace MessageFlow.Entities;

public abstract class DefaultDefinition<TInput> :
	DefaultDefinition<TInput, EndOfWorkflow, EmptyConfig> { }
public abstract class DefaultDefinition<TInput, TOutput> :
	DefaultDefinition<TInput, TOutput, EmptyConfig> { }
public abstract class DefaultDefinition<TInput, TOutput, TConfig> :
	IWorkerDefinition<TInput, TOutput, TConfig> {

	public TConfig? Config { get; set; }
	public string? SerializedConfig { get; set; }
	public virtual string? Project { get; } = null;
	public virtual string Action => GetActionName();
	public virtual string? ActionVariant { get; } = null;

	public virtual string Exchange => Project ?? "Main";
	public virtual string QueueName => GetQueueName();
	public virtual string RoutingKey => GetRoutingKey();

	public string InputTypeName => typeof(TInput).Name;

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
