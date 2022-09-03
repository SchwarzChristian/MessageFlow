using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageFlow.Entities;
using MessageFlow.Exceptions;
using MessageFlow.Workflow;

namespace MessageFlow.Connection;

/// <summary>
/// base class for worker with no output and configuration
/// </summary>
public abstract class WorkerBase<TInput> : WorkerBase<TInput, EndOfWorkflow, EmptyConfig> { }

/// <summary>
/// base class for worker without config
/// </summary>
public abstract class WorkerBase<TInput, TOutput> : WorkerBase<TInput, TOutput, EmptyConfig> { }

/// <summary>
/// base class for workers
/// </summary>
public abstract class WorkerBase<TInput, TOutput, TConfig> : IWorker, IDisposable {
	private IConnector? connector { get; set; }
	private bool disposedValue;

	public abstract IWorkerDefinition<TInput, TOutput, TConfig> Definition { get; }

	/// <summary>
	/// message that is currently processed; null, if there is currently no message processed
	/// </summary>
	protected Message<TInput>? CurrentMessage { get; private set; }

	/// <summary>
	/// start the worker
	/// </summary>
	public void Run(IConnector connector) {
		this.connector = connector;
		connector.Consume(Definition, HandleNewMessage);
	}

	internal void HandleNewMessage(BasicDeliverEventArgs args) {
		if (connector is null) throw new InvalidOperationException(
			"The worker is not connected to RabbitMQ"
		);
		var message = GetMessage(args);
		var config = GetConfig(message.CurrentStep);
		var newHistory = message.History.Concat(new [] { message.CurrentStep }).ToArray();
		var newCurrentStep = message.PendingSteps.FirstOrDefault();
		var newPendingSteps = message.PendingSteps.Skip(1).ToArray();

		CurrentMessage = message;
		bool doRequeueMessage = false;
		try {
			var results = Process(message.Content, config);
			foreach (var result in results) {
				if (newCurrentStep is null) continue;
				var newMessage = new Message<TOutput> {
					Content = result,
					CurrentStep = newCurrentStep,
					History = newHistory,
					PendingSteps = newPendingSteps,
					NamedWorkflows = message.NamedWorkflows,
					WorkflowStartedAt = message.WorkflowStartedAt,
				};
				connector.Publish(newMessage);
			}
		} catch (Exception ex) {
			connector.PublishError(message, ex);
			if (ex is RecoverableException) doRequeueMessage = true;
		}

		CurrentMessage = null;
		if (doRequeueMessage) {
			connector.Reject(args.DeliveryTag);
			return;
		}

		connector.Ack(args.DeliveryTag);
	}

	/// <summary>
	/// start a sub-workflow; this only works if the input message contains a named
	/// workflow with the given name
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// if the worker is not connected to RabbitMQ or if the worker is not processing
	/// a message right now
	/// </exception>
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

	/// <summary>
	/// start a sub-workflow with hard-coded steps; you should prefer to use
	/// <see cref="BranchWorkflow(string, T)" /> to be able to define the sub-workflow
	/// together with the main workflow
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// if the worker is not connected to RabbitMQ or if the worker is not processing
	/// a message right now
	/// </exception>
	protected void BranchWorkflow<T>(ICollection<WorkflowStep>? steps, T content) {
		if (connector is null) throw new InvalidOperationException(
			"Worker is not connected to RabbitMQ!"
		);
		if (CurrentMessage is null) throw new InvalidOperationException(
			"Worker is not processing a message, currently!"
		);

		if (steps is null) return;
		if (!steps.Any()) return;

		var message = new Message<T> {
			Content = content,
			CurrentStep = steps.First(),
			PendingSteps = steps.Skip(1).ToArray(),
			History = CurrentMessage!.History,
			NamedWorkflows = CurrentMessage!.NamedWorkflows,
			WorkflowStartedAt = CurrentMessage!.WorkflowStartedAt,
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
			connector?.Dispose();
		}

		disposedValue = true;
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// process a single message
	/// </summary>
	public abstract IEnumerable<TOutput> Process(TInput input, TConfig? config);
}
