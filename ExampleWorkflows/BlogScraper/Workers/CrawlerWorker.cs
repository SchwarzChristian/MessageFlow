using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;
using RestSharp;

namespace ExampleWorkflows.BlogScraper.Workers;
using Definition = IWorkerDefinition<CrawlingTask, CrawlResult, EmptyConfig>;

public class CrawlWorker : WorkerBase<CrawlingTask, CrawlResult, EmptyConfig> {
	public override Definition Definition { get; } = new CrawlDefinition();

	private readonly TimeSpan minInterval = TimeSpan.FromSeconds(1);
	private DateTime lastRequest = DateTime.MinValue;
	private RestClient rest;

	public CrawlWorker() {
		rest = new RestClient();
	}

	public override IEnumerable<CrawlResult> Process(
		CrawlingTask input,
		EmptyConfig? config
	) {
		if (DateTime.Now < lastRequest + minInterval) {
			Thread.Sleep(minInterval - (DateTime.Now - lastRequest));
		}
		
		Console.WriteLine($"crawling {input.Url}");
		var request = new RestRequest(input.Url, Method.Get);
		var response = rest.Execute(request);
		lastRequest = DateTime.Now;
		yield return new CrawlResult {
			Url = input.Url,
			StatusCode = response.StatusCode,
			Html = response.Content,
		};
	}
}
