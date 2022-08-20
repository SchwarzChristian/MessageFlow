using RabbitMQ.Client;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public class Setup {
	private readonly IConnector connector;

	public Setup(IConnector connector) {
		this.connector = connector;
	}

	public void SetupWorkflow<T>(Workflow<T> workflow) {
		foreach (var step in workflow.Steps) {
			SetupExchange(step);
			SetupQueue(step);
		}
	}

	public void SetupExchange(IWorkerDefinition definition) =>
		SetupExchange(connector.GetExchangeName(definition));

	public void SetupExchange(string name, string type = ExchangeType.Direct) {
		using var chan = connector.OpenChannel();
		chan.ExchangeDeclare(name, type, durable: true);
	}

	public void SetupQueue(IWorkerDefinition definition) => SetupQueue(
		name: connector.GetQueueName(definition),
		routingKey: connector.GetRoutingKey(definition),
		exchange: connector.GetExchangeName(definition)
	);

	public void SetupQueue(string name, string routingKey, string exchange) {
		using var chan = connector.OpenChannel();
		chan.QueueDeclare(
			name,
			durable: true,
			exclusive: false,
			autoDelete: false
		);

		chan.QueueBind(name, exchange, routingKey);
	}
}
