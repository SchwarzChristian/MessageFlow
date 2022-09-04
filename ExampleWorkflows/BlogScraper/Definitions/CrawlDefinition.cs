using ExampleWorkflows.BlogScraper.Entities;
using MessageFlow.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class CrawlDefinition : DefaultDefinition<CrawlingTask, CrawlResult> {
    public override string Project { get; } = nameof(BlogScraper);
}
