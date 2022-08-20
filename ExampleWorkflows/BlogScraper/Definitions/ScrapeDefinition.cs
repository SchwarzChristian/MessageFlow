using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class ScrapeDefinition : DefaultDefinition<CrawlResult, BlogPost> {
	public override string? Project => nameof(BlogScraper);
}
