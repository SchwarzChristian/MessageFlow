using RabbitMQ.Client;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public class Setup {
	private readonly IConnector connector;

	public Setup(IConnector connector) {
		this.connector = connector;
	}

	public void SetupWorkflow<T>(Workflow<T> workflow) {
		workflow.Steps.Consume(SetupQueue);
	}

	public void SetupExchange(IWorkerDefinition definition) =>
		SetupExchange(definition.Exchange);

	public void SetupExchange(string name, string type = ExchangeType.Direct) {
		using var chan = connector.OpenChannel();
		chan.ExchangeDeclare(name, type, durable: true);
	}

	public void SetupQueue(IWorkerDefinition definition) => SetupQueue(
		name: definition.QueueName,
		routingKey: definition.RoutingKey,
		exchange: definition.Exchange
	);

	public void SetupQueue(string name, string routingKey, string exchange) {
		SetupExchange(exchange);
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
