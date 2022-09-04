using Newtonsoft.Json;

namespace MessageFlow.Workflow;

/// <summary>
/// workflow step as it will be used in messages
/// </summary>
public class WorkflowStep {
	/// <summary>
	/// exchange to publish messages for this step to
	/// </summary>
    public string Exchange { get; set; } = null!;

	/// <summary>
	/// routing key with which to publish messages for this step
	/// </summary>
	public string RoutingKey { get; set; } = null!;

	internal string QueueName { get; set; } = null!;

	public string? Config { get; set; }

	internal static WorkflowStep FromWorkerDefinition<TInput, TOutput, TConfig>(
		IWorkerDefinition<TInput, TOutput, TConfig> definition,
		string? environmentPrefix
	) {
		var step = FromWorkerDefinition(definition, environmentPrefix);
		step.Config = JsonConvert.SerializeObject(definition.Config);
		return step;
	}

	internal static WorkflowStep FromWorkerDefinition(
		IWorkerDefinition definition,
		string? environmentPrefix
	) {
		if (environmentPrefix is null) return new WorkflowStep {
			Exchange = definition.Exchange,
			RoutingKey = definition.RoutingKey,
			QueueName = definition.QueueName,
			Config = definition.SerializedConfig,
		};

		return new WorkflowStep {
			Exchange = $"{environmentPrefix}.{definition.Exchange}",
			RoutingKey = definition.RoutingKey,
			QueueName = $"{environmentPrefix}.{definition.QueueName}",
			Config = definition.SerializedConfig,
		};
	}
}
