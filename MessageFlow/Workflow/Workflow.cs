using MessageFlow.Entities;

namespace MessageFlow.Workflow;

public class Workflow<TFirstInput> {
	public ICollection<IWorkerDefinition> Steps { get; } = new List<IWorkerDefinition>();
	
	public ICollection<NamedWorkflow> NamedWorkflows { get; } =
		new List<NamedWorkflow>();

	public WorkflowBuilder<TFirstInput, TFirstInput> Define() =>
		new WorkflowBuilder<TFirstInput, TFirstInput>(this);

	public void AddNamedWorkflow<T>(string name, Workflow<T> workflow) {
		NamedWorkflows.Add(new NamedWorkflow { 
			Name = name,
			Workflow = workflow.Steps
				.Select(WorkflowStep.FromWorkerDefinition)
				.ToArray(),
		});
	}
}
