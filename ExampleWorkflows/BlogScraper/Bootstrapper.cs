using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using ExampleWorkflows.BlogScraper.Workers;
using RabbitMqConnector;
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

    public void Run(Connector connector, WorkerStarter starter) {
        starter.StartWorkers(
            new CrawlWorker(),
            new FindNextPageWorker(),
            new ScrapeWorker(),
            new PersistWorker()
        );
        var task = new CrawlingTask {
            Url = new Uri("http://blog.fefe.de/"),
        };
        connector.Publish(workflow, task);
    }
}