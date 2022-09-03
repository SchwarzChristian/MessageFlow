using MessageFlow.Entities;
using MessageFlow.Workflow;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageFlow.Connection;

internal class Consumer : IDisposable {
	private readonly IConnector connector;
	private readonly ICollection<IModel> openChannels = new List<IModel>();
	private bool disposedValue;

	public Consumer(IConnector connector) {
		this.connector = connector;
	}

	public void Consume(
		IWorkerDefinition workerDefinition,
		Action<BasicDeliverEventArgs> action
	) {
		connector.SetupQueue(workerDefinition);
		var channel = connector.OpenChannel();
		openChannels.Add(channel);
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (_, args) => action(args);
		var queueName = workerDefinition.QueueName;
		if (connector.Config.EnvironmentMode == EnvironmentMode.NamePrefix) {
			queueName = $"{connector.Config.Environment}.{queueName}";
		}
		
		channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposedValue) {
			return;
		}

		if (disposing) {
			openChannels.Consume(ch => ch.Dispose());
		}

		disposedValue = true;
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
