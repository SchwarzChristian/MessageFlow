using RabbitMQ.Client;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public interface IConnector : IDisposable {
	string Environment { get; }

	IModel OpenChannel();

	void SetupQueue(IWorkerDefinition definition);
	void SetupWorkflow<T>(Workflow<T> workflow);
	void Publish<T>(Workflow<T> target, T content);
	void Publish<T>(Message<T> message);
}
