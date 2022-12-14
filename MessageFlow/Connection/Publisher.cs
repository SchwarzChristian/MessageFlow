using System.Text;
using Newtonsoft.Json;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

internal class Publisher {
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
		string? namePrefix = null;
		if (connector.Config.EnvironmentMode == EnvironmentMode.NamePrefix) {
			namePrefix = connector.Config.Environment;
		}

		var message = new Message<T> {
			RunId = Guid.NewGuid(),
			BranchId = Guid.NewGuid(),
			Content = content,
			CurrentStep = WorkflowStep.FromWorkerDefinition(firstStep, namePrefix),
			PendingSteps = target.Steps
				.Skip(1)
				.Select(it => WorkflowStep.FromWorkerDefinition(it, namePrefix))
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
