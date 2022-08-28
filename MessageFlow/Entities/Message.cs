using MessageFlow.Workflow;

namespace MessageFlow.Entities;

/// <summary>
/// message passed to the workers in workflows
/// </summary>
public class Message<T> {
	/// <summary>
	/// payload
	/// </summary>
	public T Content { get; set; } = default!;

	/// <summary>
	/// information about the workflow step the message represents
	/// </summary>
	public WorkflowStep CurrentStep { get; set; } = null!;

	/// <summary>
	/// steps which need to be processed in the workflow
	/// </summary>
	public ICollection<WorkflowStep> PendingSteps { get; set; }
		= new List<WorkflowStep>();

	/// <summary>
	/// steps which was processed to create this message
	/// </summary>
	public ICollection<WorkflowStep> History { get; set; }
		= new List<WorkflowStep>();

	/// <summary>
	/// sub-workflow definitions available to all workers in the workflow
	/// </summary>
	public ICollection<NamedWorkflow> NamedWorkflows { get; set; } =
		new List<NamedWorkflow>();

	/// <summary>
	/// when the workflow was started
	/// </summary>
	public DateTime WorkflowStartedAt { get; set; }

	/// <summary>
	/// when this message was created
	/// </summary>
	public DateTime PublishedAt { get; set; }
}
