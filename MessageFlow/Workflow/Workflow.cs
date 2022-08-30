using MessageFlow.Entities;

namespace MessageFlow.Workflow;

/// <summary>
/// workflow definition
/// </summary>
/// <typeparam name="TFirstInput">input type of the first worker in the workflow</typeparam>
public class Workflow<TFirstInput> {
	private readonly RabbitMqConfig config;

	public ICollection<IWorkerDefinition> Steps { get; } = new List<IWorkerDefinition>();
	
	/// <summary>
	/// sub-workflow definitions which will be available to all workers in the workflow
	/// </summary>
	public ICollection<NamedWorkflow> NamedWorkflows { get; } =
		new List<NamedWorkflow>();

	public Workflow(RabbitMqConfig config) {
		this.config = config;
	}

	/// <summary>
	/// returns a builder that provides a type-safe interface to define the steps in the workflow
	/// </summary>
	public WorkflowBuilder<TFirstInput, TFirstInput> Define() {
		Steps.Clear();
		return new WorkflowBuilder<TFirstInput, TFirstInput>(this);
	}

	public void AddNamedWorkflow<T>(string name, Workflow<T> workflow) {
		string? namePrefix = null;
		if (config.EnvironmentMode == EnvironmentMode.NamePrefix) {
			namePrefix = config.Environment;
		}

		NamedWorkflows.Add(new NamedWorkflow { 
			Name = name,
			Workflow = workflow.Steps
				.Select(it => WorkflowStep.FromWorkerDefinition(it, namePrefix))
				.ToArray(),
		});
	}
}
