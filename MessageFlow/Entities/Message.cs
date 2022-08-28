using MessageFlow.Workflow;

namespace MessageFlow.Entities;

public class Message<T> {
	public T Content { get; set; } = default!;
	public WorkflowStep CurrentStep { get; set; } = null!;

	public ICollection<WorkflowStep> PendingSteps { get; set; }
		= new List<WorkflowStep>();

	public ICollection<WorkflowStep> History { get; set; }
		= new List<WorkflowStep>();

	public ICollection<NamedWorkflow> NamedWorkflows { get; set; } =
		new List<NamedWorkflow>();

	public DateTime WorkflowStartedAt { get; set; }
	public DateTime PublishedAt { get; set; }
}
