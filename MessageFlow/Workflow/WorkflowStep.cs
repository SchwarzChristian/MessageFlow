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

	public string? Config { get; set; }

	public static WorkflowStep FromWorkerDefinition<TInput, TOutput, TConfig>(
		IWorkerDefinition<TInput, TOutput, TConfig> definition
	) {
		var step = FromWorkerDefinition(definition);
		step.Config = JsonConvert.SerializeObject(definition.Config);
		return step;
	}

	public static WorkflowStep FromWorkerDefinition(
		IWorkerDefinition definition
	) => new WorkflowStep {
		Exchange = definition.Exchange,
		RoutingKey = definition.RoutingKey,
		Config = definition.SerializedConfig,
	};
}
