using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using HtmlAgilityPack;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace ExampleWorkflows.BlogScraper.Workers;
using Definition = IWorkerDefinition<CrawlResult, BlogPost, EmptyConfig>;

public class ScrapeWorker : WorkerBase<CrawlResult, BlogPost, EmptyConfig> {
	public override Definition Definition => new ScrapeDefinition();

	public override IEnumerable<BlogPost> Process(
		CrawlResult input,
		EmptyConfig? config
	) {
		var doc = new HtmlDocument();
		doc.LoadHtml(input.Html);
		var posts = doc.DocumentNode.SelectNodes("//html/*");
		var queue = new Queue<HtmlNode>(posts);
		while (queue.Peek().Name != "h3") queue.Dequeue();
		while (queue.Peek().Name == "h3") {
			var dateTag = queue.Dequeue();
			var contentTag = queue.Dequeue();
			yield return new BlogPost {
				PublicationDate = DateTime.Parse(dateTag.InnerText),
				Text = contentTag.InnerText,
			};
		}
	}
}
