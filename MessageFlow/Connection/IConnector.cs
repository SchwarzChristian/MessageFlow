using RabbitMQ.Client;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

/// <summary>
/// provides connection to the RabbitMQ API; you should not need to use it directly
/// </summary>
public interface IConnector : IDisposable {
	/// <summary>
	/// connection config the connector is using
	/// </summary>
	RabbitMqConfig Config { get; }

	/// <summary>
	/// name of the exchange to publish error messages to
	/// </summary>
	string ErrorExchangeName { get; }

	IModel OpenChannel();

	/// <summary>
	/// create queue and exchange (if it does not already exist), based on the given
	/// worker definition
	/// </summary>
	void SetupQueue(IWorkerDefinition definition);

	/// <summary>
	/// creates all queues and exchanges needed for the given workflow
	/// </summary>
	void SetupWorkflow<T>(Workflow<T> workflow);

	/// <summary>
	/// publish a message to start the given workflow
	/// </summary>
	void Publish<T>(Workflow<T> target, T content);

	/// <summary>
	/// publish the given message
	/// </summary>
	void Publish<T>(Message<T> message);

	/// <summary>
	/// publish an error message to the error queue
	/// </summary>
	/// <param name="message">message that caused the error</param>
	/// <param name="ex">exception that occured while processing the message</param>
	void PublishError<T>(Message<T> message, Exception ex);
}
