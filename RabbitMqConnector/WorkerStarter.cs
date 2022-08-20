using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector;

public class WorkerStarter : IDisposable {
	private bool disposedValue;
	private ICollection<IWorker> startedWorkers = new List<IWorker>();
	private readonly RabbitMqConfig config;

	public WorkerStarter(RabbitMqConfig config) {
		this.config = config;
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
		worker.Run(new Connector(config));
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
