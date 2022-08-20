using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;
using RestSharp;

namespace ExampleWorkflows.BlogScraper.Workers;
using Definition = IWorkerDefinition<CrawlingTask, CrawlResult, EmptyConfig>;

public class CrawlWorker : WorkerBase<CrawlingTask, CrawlResult, EmptyConfig> {
	private RestClient rest;

	public override Definition Definition { get; } = new CrawlDefinition();

	public CrawlWorker() {
		rest = new RestClient();
	}

	public override IEnumerable<CrawlResult> Process(
		CrawlingTask input,
		EmptyConfig? config
	) {
		var request = new RestRequest(input.Url, Method.Get);
		var response = rest.Execute(request);
		yield return new CrawlResult {
			Url = input.Url,
			StatusCode = response.StatusCode,
			Html = response.Content,
		};
	}
}
