using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Entities;

public abstract class DefaultDefinition<TInput> :
	DefaultDefinition<TInput, EndOfWorkflow, EmptyConfig> { }
public abstract class DefaultDefinition<TInput, TOutput> :
	DefaultDefinition<TInput, TOutput, EmptyConfig> { }
public abstract class DefaultDefinition<TInput, TOutput, TConfig> :
	IWorkerDefinition<TInput, TOutput, TConfig> {

	public TConfig? Config { get; set; }
	public virtual string? Project { get; } = null;
	public virtual string Action => GetActionName();
	public virtual string? ActionVariant { get; } = null;

	public string InputTypeName => typeof(TInput).Name;

	private string GetActionName() {
		return GetType().Name.Replace("Definition", "");
	}
}
