using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using HtmlAgilityPack;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace ExampleWorkflows.BlogScraper.Workers;
using Definition = IWorkerDefinition<CrawlResult, BlogPost, EmptyConfig>;

public class ScrapeWorker : WorkerBase<CrawlResult, BlogPost, EmptyConfig> {
	public override Definition Definition => new ScrapeDefinition();

	public override IEnumerable<BlogPost> Process(
		CrawlResult input,
		EmptyConfig? config
	) {
		Console.WriteLine($"scraping blog posts from {input.Url}");
		var doc = new HtmlDocument();
		doc.LoadHtml(input.Html);
		if (IsFinished(doc.DocumentNode)) yield break;

		var posts = doc.DocumentNode.SelectNodes("//html/*");
		var queue = new Queue<HtmlNode>(posts);
		while (queue.Peek().Name != "h3") {
			queue.Dequeue();
			if (!queue.Any()) yield break;
		}

		while (queue.Peek().Name == "h3") {
			var dateTag = queue.Dequeue();
			var contentTag = queue.Dequeue();
			yield return new BlogPost {
				PublicationDate = DateTime.Parse(dateTag.InnerText),
				Text = contentTag.InnerText,
			};
		}
	}

	private bool IsFinished(HtmlNode root) {
		return root.InnerText.Contains("No entries found.");
	}
}
