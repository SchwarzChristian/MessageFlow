using RabbitMqConnector.Entities;

namespace RabbitMqConnector.Workflow;

public interface IWorker<TInput> : IWorker<TInput, EndOfWorkflow, EmptyConfig> { }
public interface IWorker<TInput, TOutput> : IWorker<TInput, TOutput, EmptyConfig> { }
public interface IWorker<TInput, TOutput, TConfig> {
	public IWorkerDefinition<TInput, TOutput, TConfig> Definition { get; }
	public IEnumerable<TOutput> Process(TInput input, TConfig config);
}