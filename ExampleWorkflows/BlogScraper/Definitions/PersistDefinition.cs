using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Entities;

namespace ExampleWorkflows.BlogScraper.Definitions;

public class PersistDefinition : DefaultDefinition<BlogPost> {
	public override string? Project => nameof(BlogScraper);
}
