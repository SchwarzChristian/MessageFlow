using System.Text.RegularExpressions;
using ExampleWorkflows.BlogScraper.Definitions;
using ExampleWorkflows.BlogScraper.Entities;
using Newtonsoft.Json;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace ExampleWorkflows.BlogScraper;
using Definition = IWorkerDefinition<BlogPost, EndOfWorkflow, EmptyConfig>;

public class PersistWorker : WorkerBase<BlogPost, EndOfWorkflow, EmptyConfig> {
	private Regex filenameCleaningRegex = new Regex(
		$"[/{Path.GetInvalidFileNameChars()}]+",
		RegexOptions.Compiled
	);
	private const string outputDirectory = "./posts/";

	public override Definition Definition => new PersistDefinition();

	public override IEnumerable<EndOfWorkflow> Process(
		BlogPost input,
		EmptyConfig? config
	) {
		Console.WriteLine($"persisting post from {input.PublicationDate?.Date}");
		Directory.CreateDirectory(outputDirectory);
		var serialized = JsonConvert.SerializeObject(input);
		var filename = GetFilename(input);
		File.WriteAllText(filename, serialized);
		return Enumerable.Empty<EndOfWorkflow>();
	}

	private string GetFilename(BlogPost input) {
		string filename = outputDirectory;
		if (input.PublicationDate.HasValue) {
			filename = input.PublicationDate.Value.ToString("o");
		} else {
			filename = Guid.NewGuid().ToString();
		}

		filename = filenameCleaningRegex.Replace(filename, "_");
		return Path.Combine(outputDirectory, filename + ".json");
	}
}
