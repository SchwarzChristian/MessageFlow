using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using HtmlAgilityPack;
using MessageFlow.Connection;
using MessageFlow.Workflow;

namespace ExampleWorkflows.BlogScraper.Workers;

using IDefinition = IWorkerDefinition<CrawlResult, CrawlResult, FindNextPageConfig>;

public class FindNextPageWorker : WorkerBase<CrawlResult, CrawlResult, FindNextPageConfig> {
	public override IDefinition Definition { get; } = new FindNextPageDefinition();

	public override IEnumerable<CrawlResult> Process(
		CrawlResult input,
		FindNextPageConfig? config
	) {
		Console.WriteLine($"find next page on {input.Url}");
		var doc = new HtmlDocument();
		doc.LoadHtml(input.Html);
		var firstPageLink = GetLinksByText(doc.DocumentNode, "ganzer Monat")?.FirstOrDefault();
		if (firstPageLink is not null) {
			ScheduleNextPage(firstPageLink, input, config);
			yield break;
		}

		var nextPageLink = GetLinksByText(doc.DocumentNode, "früher")?.FirstOrDefault();
		ScheduleNextPage(nextPageLink, input, config);
		yield return input;
	}

	private void ScheduleNextPage(
		HtmlNode? linkTag,	
		CrawlResult input,
		FindNextPageConfig? config
	) {
		if (linkTag is null) return;
		if (config?.CrawlNextPageWorkflow is null) return;

		var relativeUrl = linkTag.Attributes["href"].Value;
		var absoluteUrl = new Uri(input.Url, relativeUrl);
		var crawlingTask = new CrawlingTask { Url = absoluteUrl };
		BranchWorkflow(config!.CrawlNextPageWorkflow, crawlingTask);
	}

	private HtmlNodeCollection GetLinksByText(HtmlNode root, string text) {
		return root.SelectNodes($"//a[text() = '{text}']");
	}
}
