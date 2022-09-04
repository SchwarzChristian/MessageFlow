using ExampleWorkflows.BlogScraper.Entities;
using MessageFlow.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class FindNextPageDefinition : DefaultDefinition<CrawlResult, CrawlResult, FindNextPageConfig> {
    public override string Project => nameof(BlogScraper);
}
