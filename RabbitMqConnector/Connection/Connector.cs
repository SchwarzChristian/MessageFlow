using RabbitMQ.Client;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public class Connector : IConnector, IDisposable {
	public string Environment => environment;

	private Publisher publisher;
	private Setup setup;

	private string environment;
	private ConnectionFactory connectionFactory;
	private IConnection connection;
	private bool isDisposed;

	public Connector(RabbitMqConfig config) {
		environment = config.Environment;
		connectionFactory = new ConnectionFactory {
			HostName = config.Hostname,
			Port = config.Port,
			UserName = config.Username,
			Password = config.Passwort,
			VirtualHost = config.Environment,
		};
		connection = connectionFactory.CreateConnection();
		publisher = new Publisher(this);
		setup = new Setup(this);
	}

	public IModel OpenChannel() => connection.CreateModel();

	public void SetupQueue(IWorkerDefinition definition) => setup.SetupQueue(definition);
	public void SetupWorkflow<T>(Workflow<T> workflow) =>
		setup.SetupWorkflow(workflow);

	public void Publish<T>(Message<T> message) => publisher.Publish(message);
	public void Publish<T>(Workflow<T> target, T content) =>
		publisher.Publish(target, content);

	protected virtual void Dispose(bool disposing) {
		if (isDisposed) return;
		if (disposing) {
			connection.Dispose();
		}

		isDisposed = true;
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}