namespace RabbitMqConnector.Workflow;

public class Workflow<TFirstInput> {
	public ICollection<IWorkerDefinition> Steps { get; } = new List<IWorkerDefinition>();
	public WorkflowBuilder<TFirstInput, TFirstInput> Define() =>
		new WorkflowBuilder<TFirstInput, TFirstInput>(this);
}
