using System.Net;

namespace ExampleWorkflows.BlogScraper.Entities;

public class CrawlResult
{
    public Uri Url { get; set; } = null!;
	public string? Html { get; set; }
	public HttpStatusCode StatusCode { get; set; }
}
