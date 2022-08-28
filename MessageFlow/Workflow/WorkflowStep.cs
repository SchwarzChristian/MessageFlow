using Newtonsoft.Json;

namespace MessageFlow.Workflow;

public class WorkflowStep {
    public string Exchange { get; set; } = null!;
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
