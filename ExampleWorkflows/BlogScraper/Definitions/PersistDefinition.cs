using ExampleWorkflows.BlogScraper.Entities;
using MessageFlow.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class PersistDefinition : DefaultDefinition<BlogPost> {
	public override string Project => nameof(BlogScraper);
}
