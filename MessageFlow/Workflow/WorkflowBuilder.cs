using Newtonsoft.Json;
using MessageFlow.Entities;

namespace MessageFlow.Workflow;

/// <summary>
/// defines workflow steps in a type-safe way
/// </summary>
/// <typeparam name="TFirst">input type of the first worker of the workflow</typeparam>
/// <typeparam name="TCurrent">input type of the worker for the current step</typeparam>
public class WorkflowBuilder<TFirst, TCurrent> {
	private readonly Workflow<TFirst> buildingFor;

	internal WorkflowBuilder(Workflow<TFirst> buildingFor) {
		this.buildingFor = buildingFor;
	}

	/// <summary>
	/// adds the last step of the workflow
	/// </summary>
	/// <typeparam name="TStep">worker definition for the step</typeparam>
	public void Step<TStep>()
		where TStep : IWorkerDefinition<TCurrent, EndOfWorkflow, EmptyConfig>, new() =>
		Step<TStep, EndOfWorkflow>();

	/// <summary>
	/// adds a step to the workflow
	/// </summary>
	/// <typeparam name="TStep">worker definition for the step</typeparam>
	/// <typeparam name="TNext">output type of the worker processing this step</typeparam>
	public WorkflowBuilder<TFirst, TNext> Step<TStep, TNext>()
		where TStep : IWorkerDefinition<TCurrent, TNext, EmptyConfig>, new() =>
		Step<TStep, TNext, EmptyConfig>();

	/// <summary>
	/// adds a step to the workflow
	/// </summary>
	/// <typeparam name="TStep">worker definition for the step</typeparam>
	/// <typeparam name="TNext">output type of the worker processing this step</typeparam>
	/// <typeparam name="TConfig">
	/// type of the configuration of the worker processing this step
	/// </typeparam>
	public WorkflowBuilder<TFirst, TNext> Step<TStep, TNext, TConfig>(
		TConfig? config = default
	) where TStep : IWorkerDefinition<TCurrent, TNext, TConfig>, new() {
		buildingFor.Steps.Add(new TStep { 
			Config = config,
			SerializedConfig = JsonConvert.SerializeObject(config),
		});
		return new WorkflowBuilder<TFirst, TNext>(buildingFor);
	}
}
