using FluentAssertions;
using RabbitMQ.Client;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Workflow;

namespace MessageFlow.Tests;

public class WorkerStarterTests
{
	private Mock<IModel> mockChannel = null!;
	private Mock<IConnector> mockConnector = null!;

	[SetUp]
    public void Setup() {
		mockChannel = new Mock<IModel>();
		mockConnector = new Mock<IConnector>();
		mockConnector.Setup(m => m.OpenChannel()).Returns(mockChannel.Object);
		mockConnector.Setup(m => m.Config).Returns(new RabbitMqConfig());
	}

    [Test]
    public void StartAllWorkersTest() {
		using var starter = new WorkerStarter(
            mockConnector.Object.Config,
            config => mockConnector.Object
        );

		starter.StartAllWorkers();
		Worker.InstanceCount.Should().Be(1);

		starter.StartAllWorkers();
		Worker.InstanceCount.Should().Be(1);
	}

    private class WorkerDefinition : DefaultDefinition<string> {

    }

	private class Worker : WorkerBase<string> {
		public static int InstanceCount { get; private set; } = 0;
		public override IWorkerDefinition<string, EndOfWorkflow, EmptyConfig> Definition => 
            new WorkerDefinition();

        public Worker() {
			InstanceCount += 1;
		}

		public override IEnumerable<EndOfWorkflow> Process(string input, EmptyConfig? config) {
			return Array.Empty<EndOfWorkflow>();
		}
	}
}
