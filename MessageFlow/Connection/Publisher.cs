using System.Text;
using Newtonsoft.Json;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

public class Publisher {
	private readonly IConnector connector;

	public Publisher(IConnector connector) {
		this.connector = connector;
	}

	public void Publish<T>(Workflow<T> target, T content) {
		var firstStep = target.Steps.FirstOrDefault();
		if (firstStep is null) throw new InvalidOperationException(
			"Failed to publish message for Workflow. Workflow is undefined."
		);
		var exchange = firstStep.Exchange;
		var routingKey = firstStep.RoutingKey;
		var actionName = GetActionName(firstStep);
		var message = new Message<T> {
			Content = content,
			CurrentStep = WorkflowStep.FromWorkerDefinition(firstStep),
			PendingSteps = target.Steps
				.Skip(1)
				.Select(it => WorkflowStep.FromWorkerDefinition(it))
				.ToArray(),
			NamedWorkflows = target.NamedWorkflows,
			WorkflowStartedAt = DateTime.Now,
		};
		Publish(message);
	}

	private string GetActionName(
		IWorkerDefinition target
	) {
		if (string.IsNullOrWhiteSpace(target.ActionVariant)) return target.Action;
		return $"{target.Action} ({target.ActionVariant})";
	}

	public void Publish<T>(Message<T> message) {
		message.PublishedAt = DateTime.Now;
		var serialized = JsonConvert.SerializeObject(message);
		var binary = Encoding.UTF8.GetBytes(serialized);
		var step = message.CurrentStep;
		Publish(step.Exchange, step.RoutingKey, binary);
	}

	internal void PublishError<T>(Message<T> message, Exception ex) {
		var error = new Error<T> {
			CreatedAt = DateTime.Now,
			Message = message,
			Problem = ex.Message,
			StackTrace = ex.StackTrace?.Split("\n") ?? Array.Empty<string>(),
			Type = ex.GetType().Name,
		};
		var serialized = JsonConvert.SerializeObject(error);
		var binary = Encoding.UTF8.GetBytes(serialized);

		Publish(connector.ErrorExchangeName, string.Empty, binary);
	}

	internal void Publish(string exchange, string routingKey, byte[] data) {
		using var chan = connector.OpenChannel();
		chan.BasicPublish(
			exchange,
			routingKey,
			mandatory: false,
			basicProperties: null,
			body: data
		);
	}
}
