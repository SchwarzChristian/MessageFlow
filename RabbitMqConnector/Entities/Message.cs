using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Entities;

public class Message<T> {
	public T Content { get; set; } = default!;
	public WorkflowStep CurrentStep { get; set; } = null!;

	public ICollection<WorkflowStep> PendingSteps { get; set; }
		= new List<WorkflowStep>();

	public ICollection<WorkflowStep> History { get; set; }
		= new List<WorkflowStep>();
}
