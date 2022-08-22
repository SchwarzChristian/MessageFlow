using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Entities;

public class NamedWorkflow {
	public string Name { get; set; } = "<undefined>";
	public ICollection<WorkflowStep> Workflow { get; set; } =
		new List<WorkflowStep>();
}
