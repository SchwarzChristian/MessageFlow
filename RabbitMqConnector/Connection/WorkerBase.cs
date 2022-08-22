using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Connection;

public abstract class WorkerBase<TInput, TOutput, TConfig> : IWorker, IDisposable {
	public IConnector? connector { get; set; }
	private IModel? channel;
	private bool disposedValue;

	public abstract IWorkerDefinition<TInput, TOutput, TConfig> Definition { get; }
	protected Message<TInput>? CurrentMessage { get; private set; }

	public void Run(IConnector connector) {
		this.connector = connector;
		channel = connector.OpenChannel();
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += HandleNewMessage;
		connector.SetupQueue(Definition);
		channel.BasicConsume(
			queue: Definition.QueueName,
			autoAck: false,
			consumer: consumer
		);
	}

	private void HandleNewMessage(object? sender, BasicDeliverEventArgs args) {
		var message = GetMessage(args);
		var config = GetConfig(message.CurrentStep);
		var newHistory = message.History.Concat(new [] { message.CurrentStep }).ToArray();
		var newCurrentStep = message.PendingSteps.FirstOrDefault();
		var newPendingSteps = message.PendingSteps.Skip(1).ToArray();

		CurrentMessage = message;
		var results = Process(message.Content, config);

		foreach (var result in results) {
			if (newCurrentStep is null) continue;
			var newMessage = new Message<TOutput> {
				Content = result,
				CurrentStep = newCurrentStep,
				History = newHistory,
				PendingSteps = newPendingSteps,
				NamedWorkflows = message.NamedWorkflows,
			};
			connector!.Publish(newMessage);
		}

		CurrentMessage = null;
		channel!.BasicAck(args.DeliveryTag, multiple: false);
	}

	protected void BranchWorkflow<T>(string? workflowName, T content) {
		if (workflowName is null) return;
		if (CurrentMessage is null) throw new InvalidOperationException(
			"Worker is not processing a message, currently!"
		);

		var workflow = CurrentMessage.NamedWorkflows
			.FirstOrDefault(nw => nw.Name == workflowName)?
			.Workflow;
		BranchWorkflow(workflow, content);
	}

	protected void BranchWorkflow<T>(ICollection<WorkflowStep>? steps, T content) {
		if (connector is null) throw new InvalidOperationException(
			"Worker is not connected to RabbitMQ!"
		);
		if (CurrentMessage is null) throw new InvalidOperationException(
			"Worker is not processing a message, currently!"
		);
		bool hasSteps = steps?.Any() ?? false;
		if (!hasSteps) return;

		var message = new Message<T> {
			Content = content,
			CurrentStep = steps!.First(),
			PendingSteps = steps!.Skip(1).ToArray(),
			History = CurrentMessage?.History ?? Array.Empty<WorkflowStep>(),
			NamedWorkflows = CurrentMessage!.NamedWorkflows,
		};
		connector.Publish(message);
	}

	private Message<TInput> GetMessage(BasicDeliverEventArgs args) {
		var binary = args.Body.ToArray();
		var serialized = Encoding.UTF8.GetString(binary);
		return JsonConvert.DeserializeObject<Message<TInput>>(serialized)!;
	}

	private TConfig? GetConfig(WorkflowStep step) {
		if (step.Config is null) return default;
		return JsonConvert.DeserializeObject<TConfig>(step.Config);
	}

	protected virtual void Dispose(bool disposing) {
		if (disposedValue) return;
		if (disposing) {
			channel?.Dispose();
			connector?.Dispose();
		}

		disposedValue = true;
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public abstract IEnumerable<TOutput> Process(TInput input, TConfig? config);
}
