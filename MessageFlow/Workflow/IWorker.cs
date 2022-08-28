using MessageFlow.Connection;

namespace MessageFlow.Workflow;

public interface IWorker : IDisposable {
	public void Run(IConnector connector);
}