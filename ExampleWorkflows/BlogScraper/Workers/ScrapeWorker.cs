using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace ExampleWorkflows.BlogScraper.Workers;

public class ScrapeWorker : IWorker<CrawlResult, BlogPost> {
	public IWorkerDefinition<CrawlResult, BlogPost, EmptyConfig> Definition => 
		new ScrapeDefinition();

	public IEnumerable<BlogPost> Process(CrawlResult input, EmptyConfig config) {
		throw new NotImplementedException();
	}
}
