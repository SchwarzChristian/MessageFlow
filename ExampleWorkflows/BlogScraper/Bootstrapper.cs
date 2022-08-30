using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using ExampleWorkflows.BlogScraper.Workers;
using MessageFlow;
using MessageFlow.Connection;
using MessageFlow.Workflow;

namespace ExampleWorkflows.BlogScraper;

public class Bootstrapper : IWorkflowBootstrapper {
    private const string findNextPageWorkflowName = "findNextPage";

    public void Run(Connector connector, WorkerStarter starter) {
        var findNextPageConfig = new FindNextPageConfig {
            CrawlNextPageWorkflow = findNextPageWorkflowName,
        };
        var workflow = new Workflow<CrawlingTask>(connector.Config);
        workflow.Define()
            .Step<CrawlDefinition, CrawlResult>()
            .Step<FindNextPageDefinition, CrawlResult, FindNextPageConfig>(findNextPageConfig)
            .Step<ScrapeDefinition, BlogPost>()
            .Step<PersistDefinition>();

        workflow.AddNamedWorkflow(findNextPageWorkflowName, workflow);
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
