using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using Newtonsoft.Json;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace ExampleWorkflows.BlogScraper;

public class PersistWorker : IWorker<BlogPost> {
	private Regex filenameCleaningRegex = new Regex(
		$"[{Path.GetInvalidFileNameChars()}]+",
		RegexOptions.Compiled
	);
	private const string outputDirectory = "./posts/";

	public IWorkerDefinition<BlogPost, EndOfWorkflow, EmptyConfig> Definition => 
		new PersistDefinition();

	public IEnumerable<EndOfWorkflow> Process(BlogPost input, EmptyConfig config) {
		var serialized = JsonConvert.SerializeObject(input);
		var filename = GetFilename(input);
		File.WriteAllText(filename, serialized);
		return Enumerable.Empty<EndOfWorkflow>();
	}

	private string GetFilename(BlogPost input) {
		string filename = outputDirectory;
		if (input.PublicationDate.HasValue) {
			filename = input.PublicationDate.Value.ToString();
		} else {
			filename = Guid.NewGuid().ToString();
		}

		filename = filenameCleaningRegex.Replace(filename, "_");
		return Path.Combine(outputDirectory, filename + ".json");
	}
}
