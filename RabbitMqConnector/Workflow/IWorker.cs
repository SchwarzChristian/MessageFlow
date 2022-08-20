using RabbitMqConnector.Connection;

namespace RabbitMqConnector.Workflow;

public interface IWorker : IDisposable {
	public void Run(IConnector connector);
}