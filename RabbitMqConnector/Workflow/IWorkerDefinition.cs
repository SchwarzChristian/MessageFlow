namespace RabbitMqConnector.Workflow;

public interface IWorkerDefinition<TInput, TOutput, TConfig> : IWorkerDefinition {
	public TConfig? Config { get; set; }
}

public interface IWorkerDefinition {
	public string? Project { get; }
	public string Action { get; }
	public string? ActionVariant { get; }
	public string InputTypeName { get; }
}