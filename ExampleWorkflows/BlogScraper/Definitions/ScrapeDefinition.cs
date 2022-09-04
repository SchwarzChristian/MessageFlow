using ExampleWorkflows.BlogScraper.Entities;
using MessageFlow.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class ScrapeDefinition : DefaultDefinition<CrawlResult, BlogPost> {
	public override string Project => nameof(BlogScraper);
}
