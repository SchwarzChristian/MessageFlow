using MessageFlow.Connection;

namespace MessageFlow.Workflow;

/// <summary>
/// main interface for all workers
/// </summary>
public interface IWorker : IDisposable {
	/// <summary>
	/// starts the worker
	/// </summary>
	public void Run(IConnector connector);
}