using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleWorkflows.BlogScraper.Entities;

public class BlogPost
{
    public string? Title { get; set; }
	public DateTime? PublicationDate { get; set; }
	public string? Text { get; set; }
}
