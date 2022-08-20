using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;
using RestSharp;

namespace ExampleWorkflows.BlogScraper.Workers;

public class CrawlWorker : IWorker<CrawlingTask, CrawlResult> {
	private RestClient rest;

	public IWorkerDefinition<CrawlingTask, CrawlResult, EmptyConfig> Definition { get; } =
		new CrawlDefinition();

	public CrawlWorker() {
		rest = new RestClient();
	}

	public IEnumerable<CrawlResult> Process(CrawlingTask input, EmptyConfig config) {
		var request = new RestRequest(input.Url, Method.Get);
		var response = rest.Execute(request);
		yield return new CrawlResult {
			Url = input.Url,
			StatusCode = response.StatusCode,
			Html = response.Content,
		};
	}
}
