using RabbitMQ.Client;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

public interface IConnector : IDisposable {
	string Environment { get; }
	string ErrorExchangeName { get; }

	IModel OpenChannel();

	void SetupQueue(IWorkerDefinition definition);
	void SetupWorkflow<T>(Workflow<T> workflow);
	void Publish<T>(Workflow<T> target, T content);
	void Publish<T>(Message<T> message);
	void PublishError<T>(Message<T> message, Exception ex);
}
