using RabbitMQ.Client;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public interface IConnector {
	string Environment { get; }

	IModel OpenChannel();

	string GetExchangeName(IWorkerDefinition target);
	string GetQueueName(IWorkerDefinition target);
	string GetRoutingKey(IWorkerDefinition target);

	void SetupWorkflow<T>(Workflow<T> workflow);
	void Publish<T>(Workflow<T> target, T content);
}
