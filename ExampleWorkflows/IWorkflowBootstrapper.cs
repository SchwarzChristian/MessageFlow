using MessageFlow;
using MessageFlow.Connection;

namespace ExampleWorkflows;

public interface IWorkflowBootstrapper {
	void Run(Connector connector, WorkerStarter starter);
}
