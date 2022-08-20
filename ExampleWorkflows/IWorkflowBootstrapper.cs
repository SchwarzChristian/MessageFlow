using RabbitMqConnector;
using RabbitMqConnector.Connection;

namespace ExampleWorkflows;

public interface IWorkflowBootstrapper {
	void Run(Connector connector, WorkerStarter starter);
}
