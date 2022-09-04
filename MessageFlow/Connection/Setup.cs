using RabbitMQ.Client;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

internal class Setup {
	private readonly IConnector connector;

	public Setup(IConnector connector) {
		this.connector = connector;
	}

	public void SetupWorkflow<T>(Workflow<T> workflow) {
		workflow.Steps.Consume(SetupQueue);
	}

	public void SetupQueue(IWorkerDefinition definition) {
		string? namePrefix = null;
		if (connector.Config.EnvironmentMode == EnvironmentMode.NamePrefix) {
			namePrefix = connector.Config.Environment;
		}

		var step = WorkflowStep.FromWorkerDefinition(definition, namePrefix);

		SetupQueue(
			name: step.QueueName,
			routingKey: step.RoutingKey,
			exchange: step.Exchange
		);
	}

	internal void SetupExchange(string name, string type = ExchangeType.Topic) {
		using var chan = connector.OpenChannel();
		chan.ExchangeDeclare(name, type, durable: true);
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
