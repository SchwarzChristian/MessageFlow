using MessageFlow.Workflow;

namespace MessageFlow.Entities;

/// <summary>
/// sub-workflow definition
/// </summary>
public class NamedWorkflow {
	public string Name { get; set; } = "<undefined>";
	public ICollection<WorkflowStep> Workflow { get; set; } =
		new List<WorkflowStep>();
}
