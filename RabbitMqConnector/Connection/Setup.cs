using RabbitMQ.Client;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public class Setup {
	private readonly IConnector connector;
	private readonly EnvironmentMode environmentMode;

	public Setup(IConnector connector, EnvironmentMode environmentMode) {
		this.connector = connector;
		this.environmentMode = environmentMode;
	}

	public void SetupWorkflow<T>(Workflow<T> workflow) {
		workflow.Steps.Consume(SetupQueue);
	}

	internal void SetupExchange(string name, string type = ExchangeType.Topic) {
		using var chan = connector.OpenChannel();
		chan.ExchangeDeclare(name, type, durable: true);
	}

	public void SetupQueue(IWorkerDefinition definition) {
		var exchange = definition.Exchange;
		var queueName = definition.QueueName;
		var routingKey = definition.RoutingKey;

		if (environmentMode == EnvironmentMode.NamePrefix) {
			exchange = $"{connector.Environment}.{exchange}";
			queueName = $"{connector.Environment}.{queueName}";
			routingKey = $"{connector.Environment}.{routingKey}";
		}

		SetupQueue(
			name: queueName,
			routingKey: routingKey,
			exchange: exchange
		);
	}

	internal void SetupQueue(
		string name,
		string routingKey,
		string exchange,
		string exchangeType = ExchangeType.Topic
	) {
		SetupExchange(exchange, exchangeType);
		using var chan = connector.OpenChannel();
		chan.QueueDeclare(
			name,
			durable: true,
			exclusive: false,
			autoDelete: false
		);

		chan.QueueBind(name, exchange, routingKey);
	}

	internal void SetupErrorQueue() {
		var name = connector.ErrorExchangeName;
		SetupQueue(
			name: name,
			routingKey: string.Empty,
			exchange: name,
			exchangeType: ExchangeType.Fanout
		);
	}
}
