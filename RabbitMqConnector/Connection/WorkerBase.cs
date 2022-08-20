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
		var results = Process(message.Content, config);
		if (!message.PendingSteps.Any()) return;

		var newHistory = message.History.Concat(new [] { message.CurrentStep }).ToArray();
		var newCurrentStep = message.PendingSteps.First();
		var newPendingSteps = message.PendingSteps.Skip(1).ToArray();
		channel!.BasicAck(args.DeliveryTag, multiple: false);
		foreach (var result in results) {
			var newMessage = new Message<TOutput> {
				Content = result,
				CurrentStep = newCurrentStep,
				History = newHistory,
				PendingSteps = newPendingSteps,
			};
			connector!.Publish(newMessage);
		}
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
