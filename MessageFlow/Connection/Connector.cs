using RabbitMQ.Client;
using MessageFlow.Entities;
using MessageFlow.Workflow;
using RabbitMQ.Client.Events;

namespace MessageFlow.Connection;

public class Connector : IConnector, IDisposable {
	public RabbitMqConfig Config { get; }
	public string ErrorExchangeName { get {
		if (Config.EnvironmentMode == EnvironmentMode.NamePrefix) {
			return Config.Environment + ".errors";
		}

		return "errors";
	}}

	private Publisher publisher;
	private Setup setup;
	private Consumer consumer;

	private ConnectionFactory connectionFactory;
	private IConnection connection;
	private bool isDisposed;

	public Connector(RabbitMqConfig config) {
		Config = config;
		connectionFactory = new ConnectionFactory {
			HostName = config.Hostname,
			Port = config.Port,
			UserName = config.Username,
			Password = config.Passwort,
		};
		if (config.EnvironmentMode == EnvironmentMode.VHost) {
			connectionFactory.VirtualHost = config.Environment;
		}

		connection = connectionFactory.CreateConnection();
		publisher = new Publisher(this);
		setup = new Setup(this);
		setup.SetupErrorQueue();
		consumer = new Consumer(this);
	}

	public IModel OpenChannel() => connection.CreateModel();

	public void SetupQueue(IWorkerDefinition definition) => setup.SetupQueue(definition);
	public void SetupWorkflow<T>(Workflow<T> workflow) =>
		setup.SetupWorkflow(workflow);

	public void PublishError<T>(Message<T> message, Exception ex) => 
		publisher.PublishError<T>(message, ex);

	public void Publish<T>(Message<T> message) => 
		publisher.Publish(message);
	public void Publish<T>(Workflow<T> target, T content) =>
		publisher.Publish(target, content);

	public void Consume(
		IWorkerDefinition workerDefinition,
		Action<BasicDeliverEventArgs> args
	) => consumer.Consume(workerDefinition, args);

	public void Ack(ulong deliveryTag, bool doAckAllMessagesUntilThisTag = false) {
		using var channel = OpenChannel();
		channel.BasicAck(deliveryTag, doAckAllMessagesUntilThisTag);
	}

	public void Reject(ulong deliveryTag, bool doRejectAllMessagesUntilThisTag = false) {
		using var channel = OpenChannel();
		channel.BasicNack(deliveryTag, doRejectAllMessagesUntilThisTag, requeue: true);
	}

	protected virtual void Dispose(bool disposing) {
		if (isDisposed) return;
		if (disposing) {
			connection.Dispose();
			consumer.Dispose();
		}

		isDisposed = true;
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
