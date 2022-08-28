using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow;

public class WorkerStarter : IDisposable {
	private bool disposedValue;
	private ICollection<IWorker> startedWorkers = new List<IWorker>();
	private readonly RabbitMqConfig config;
	private readonly Func<RabbitMqConfig, IConnector> connectorFactory;

	public WorkerStarter(
		RabbitMqConfig config,
		Func<RabbitMqConfig, IConnector>? connectorFactory = null
	) {
		this.config = config;
		this.connectorFactory = connectorFactory ?? (config => new Connector(config));
	}

	public void StartAllWorkers() {
		AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(ad => ad.GetTypes())
			.Where(t => !t.IsAbstract)
			.Where(t => !t.IsInterface)
			.Where(typeof(IWorker).IsAssignableFrom)
			.Where(t => !IsWorkerAlreadyStarted(t))
			.Select(Activator.CreateInstance)
			.Cast<IWorker>()
			.Consume(StartWorker);
	}

	private bool IsWorkerAlreadyStarted(Type type) {
		return startedWorkers
			.Select(w => w.GetType())
			.Contains(type);
	}

	public void StartWorkers(params IWorker[] workers) {
		workers.Consume(StartWorker);
	}

	public void StartWorker(IWorker? worker) {
		if (worker is null) return;
		worker.Run(connectorFactory(config));
		startedWorkers.Add(worker);
	}

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue) {
			if (disposing) {
				startedWorkers.Consume(it => it.Dispose());
				startedWorkers.Clear();
			}

			disposedValue = true;
		}
	}

	public void Dispose() {
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
