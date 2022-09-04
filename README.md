# MessageFlow
Management Library for Workflows based on the RabbitMQ Message Queue System.

## Usage

1. write some worker definitions:
```csharp
public class MyWorkerDefinition : DefaultDefinition<InputType, OutputType, ConfigType> {}
```

2. write your worker:
```csharp
public class MyWorker : WorkerBase<InputType, OutputType, ConfigType> {
	public override IWorkerDefinition<sInputType, OutputType, ConfigType> Definition { get; } =
		new MyWorkerDefinition();

	public override IEnumerable<OutputType> Process(
		InputType input,
		ConfigType? config
	) {
		// your code
	}
}
```

3. start your workers:
```csharp
public void RunWorkers() {
	config = new RabbitMqConfig {
		// define your configuration
	};

	using var connector = new Connector(config);
	using var starter = new WorkerStarter(connector);

	starter.StartAllWorkers();

	// block execution until you want to stop your workers
	// once the starter disposes, it will stop all workers it has started
}
```

4. define a workflow:
```csharp
var workflow = new Workflow<FirstInputType>();
workflow.Define()
	.Step<FirstStep, SecondInputType>()
	.Step<SecondStep, FinalInputType>()
	.Step<FinalStep>();
```

5. start a workflow
```csharp
config = new RabbitMqConfig {
	// define your configuration
};

using var connector = new Connector(config);
var input = new FirstInput {
	// your input data
};
connector.Publish(workflow, input);
```

For a complete example, see: https://github.com/SchwarzChristian/MessageFlow/tree/master/ExampleWorkflows/BlogScraper