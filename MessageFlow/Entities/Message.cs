using MessageFlow.Workflow;

namespace MessageFlow.Entities;

/// <summary>
/// message passed to the workers in workflows
/// </summary>
public class Message<T> {
	/// <summary>
	/// unique identifier of this message
	/// </summary>
	public Guid MessageId { get; set; } = Guid.NewGuid();

	/// <summary>
	/// Identifier for all messages belonging to a workflow run-through. Will be generated
	/// for the first message in the worklfow and inherited by all subsequent messages.
	/// Will be null if the message does not belong to a workflow run.
	/// </summary>
	public Guid? RunId { get; set; } = null;

	/// <summary>
	/// Identifier for the workflow branch the message belongs to. Will be generated
	/// for the first message of the workflow ind inherited by all subsequent messages.
	/// When starting a new sub-workflow, this identifier will be changed.
	/// </summary>
	public Guid? BranchId { get; set; } = null;

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
