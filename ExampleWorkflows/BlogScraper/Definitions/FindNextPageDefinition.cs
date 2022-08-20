using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class FindNextPageDefinition : DefaultDefinition<CrawlResult, CrawlResult> {
    public override string? Project => nameof(BlogScraper);
}
