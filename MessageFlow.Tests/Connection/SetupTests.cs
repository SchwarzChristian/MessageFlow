using RabbitMQ.Client;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Tests.Helper;
using MessageFlow.Workflow;

namespace MessageFlow.Tests.Connection;

public class SetupTests {
	private Setup setup = null!;
	private Mock<IModel> mockChannel = null!;
	private Mock<IConnector> mockConnector = null!;

	[SetUp]
    public void Setup() {
		mockChannel = new Mock<IModel>();
		mockConnector = new Mock<IConnector>();
		mockConnector.Setup(m => m.OpenChannel()).Returns(mockChannel.Object);
		mockConnector.Setup(m => m.ErrorExchangeName).Returns("test.errors");
		mockConnector.Setup(m => m.Config).Returns(new RabbitMqConfig());
		setup = new Setup(mockConnector.Object);
	}

	[Test]
	public void SetupWorkflowTest() {
		var workflow = new TestWorkflowBuilder().BuildWorkflow();

		setup.SetupWorkflow(workflow);

		mockChannel.Verify(m => m.ExchangeDeclare(
			"prod." + new Step1().Exchange,
			It.IsAny<string>(),
			true,
			false,
			It.IsAny<IDictionary<string, object>>()
		), Times.AtLeastOnce);

		var steps = new IWorkerDefinition[] { 
			new Step1(),
			new Step2(),
			new Step3(),
		};
		foreach (var step in steps) {
			mockChannel.Verify(m => m.QueueDeclare(
				"prod." + step.QueueName,
				true,
				false,
				false,
				It.IsAny<IDictionary<string, object>>()
			), Times.Once);
			mockChannel.Verify(m => m.QueueBind(
				"prod." + step.QueueName,
				"prod." + step.Exchange,
				step.RoutingKey,
				It.IsAny<IDictionary<string, object>>()
			), Times.Once);
		}
	}

	[Test]
	public void SetupErrorQueueTest() {
		mockConnector.Setup(m => m.Config).Returns(new RabbitMqConfig());

		setup.SetupErrorQueue();

		mockChannel.Verify(m => m.ExchangeDeclare(
			"test.errors",
			ExchangeType.Fanout,
			true,
			false,
			It.IsAny<IDictionary<string, object>>()
		), Times.Once);
		mockChannel.Verify(m => m.QueueDeclare(
			"test.errors",
			true,
			false,
			false,
			It.IsAny<IDictionary<string, object>>()
		), Times.Once);
	}
}
