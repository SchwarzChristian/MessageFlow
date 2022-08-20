using RabbitMqConnector.Entities;

namespace RabbitMqConnector.Workflow;

public class WorkflowBuilder<TFirst, TCurrent> {
	private readonly Workflow<TFirst> buildingFor;

	public WorkflowBuilder(Workflow<TFirst> buildingFor) {
		this.buildingFor = buildingFor;
	}

	public WorkflowBuilder<TFirst, EndOfWorkflow> Step<TStep>()
		where TStep : IWorkerDefinition<TCurrent, EndOfWorkflow, EmptyConfig>, new() =>
		Step<TStep, EndOfWorkflow>();

	public WorkflowBuilder<TFirst, TNext> Step<TStep, TNext>()
		where TStep : IWorkerDefinition<TCurrent, TNext, EmptyConfig>, new() =>
		Step<TStep, TNext, EmptyConfig>();

	public WorkflowBuilder<TFirst, TNext> Step<TStep, TNext, TConfig>(
		TConfig? config = default
	) where TStep : IWorkerDefinition<TCurrent, TNext, TConfig>, new() {
		buildingFor.Steps.Add(new TStep { Config = config });
		return new WorkflowBuilder<TFirst, TNext>(buildingFor);
	}
}
