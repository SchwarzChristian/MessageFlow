using ExampleWorkflows;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;

internal class Program {
	private static IDictionary<string, IWorkflowBootstrapper> workflowMap = 
		new Dictionary<string, IWorkflowBootstrapper> {
			["blog-scraper"] = new ExampleWorkflows.BlogScraper.Bootstrapper(),
		};

	private static readonly RabbitMqConfig config = new RabbitMqConfig {
		Environment = "test",
		Username = "test." + nameof(ExampleWorkflows.BlogScraper),
		Passwort = "BlogScraperPassword",
	};

    private static void Main(string[] args) {
        if (args.Length < 1) {
			Usage();
			return;
		}

		var workflowName = args.First();
		if (!workflowMap.ContainsKey(workflowName)) {
			Console.Error.WriteLine($"Workflow '{workflowName}' not found!");
			Usage();
			return;
		}

		var bootstrapper = workflowMap[workflowName];
		using var connector = new Connector(config);
		bootstrapper.Run(connector);
    }

	private static void Usage() {
		Console.Out.WriteLine("usage: <workflow name>");
		Console.Out.WriteLine();
		Console.Out.WriteLine("available workflows:");
		workflowMap.Keys
			.Select(it => $"  - " + it)
			.Consume(Console.Out.WriteLine);
	}
}