using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using HtmlAgilityPack;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace ExampleWorkflows.BlogScraper.Workers;

using IDefinition = IWorkerDefinition<CrawlResult, CrawlResult, EmptyConfig>;

public class FindNextPageWorker : WorkerBase<CrawlResult, CrawlResult, EmptyConfig> {
	public override IDefinition Definition { get; } = new FindNextPageDefinition();

	public override IEnumerable<CrawlResult> Process(
		CrawlResult input,
		EmptyConfig? config
	) {
		var doc = new HtmlDocument();
		doc.LoadHtml(input.Html);
		var firstPageLink = GetLinksByText(doc.DocumentNode, "ganzer Monat");
		var hasFirstPageLink = firstPageLink?.Any() ?? false;
		if (hasFirstPageLink) {
			var relativeUrl = firstPageLink!.First().Attributes["href"].Value;
			// ToDo: [RMW-1] schedule next page crawl once branching feature is available
			yield return input;
		}

		var nextPageLink = GetLinksByText(doc.DocumentNode, "früher");
		// ToDo: [RMW-1] schedule next page crawl once branching feature is available
		yield return input;
	}

	private HtmlNodeCollection GetLinksByText(HtmlNode root, string text) {
		return root.SelectNodes($"//a[contains(text(), '{text}')]");
	}
}
