using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using RabbitMqConnector.Connection;
using RabbitMqConnector.Workflow;

namespace ExampleWorkflows.BlogScraper;

public class Bootstrapper : IWorkflowBootstrapper {
    private Workflow<CrawlingTask> workflow;

    public Bootstrapper() {
        workflow = new Workflow<CrawlingTask>();
        workflow.Define()
            .Step<CrawlDefinition, CrawlResult>()
            .Step<FindNextPageDefinition, CrawlResult>()
            .Step<ScrapeDefinition, BlogPost>()
            .Step<PersistDefinition>();
    }

    public void Run(Connector connector) {
        connector.SetupWorkflow(workflow);
        var task = new CrawlingTask {
            Url = new Uri("http://blog.fefe.de/"),
        };
        connector.Publish(workflow, task);
    }
}
