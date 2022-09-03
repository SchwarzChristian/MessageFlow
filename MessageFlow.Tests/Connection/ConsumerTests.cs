using MessageFlow.Connection;
using MessageFlow.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageFlow.Tests.Connection;

public class ConsumerTests
{
	private Mock<IConnector> mockConnector = null!;
	private Mock<IModel> mockChannel = null!;
	private Consumer consumer = null!;

	[SetUp]
    public void Setup() {
		mockConnector = new Mock<IConnector>();
		mockChannel = new Mock<IModel>();
		mockConnector.Setup(m => m.OpenChannel()).Returns(mockChannel.Object);
		mockConnector
            .Setup(m => m.Config)
            .Returns(new RabbitMqConfig { EnvironmentMode = EnvironmentMode.VHost });

		consumer = new Consumer(mockConnector.Object);
	}

    [Test]
    public void ConsumeTest() {
		var definition = new Definition();
        
        consumer.Consume(definition, _ => { });

		mockConnector.Verify(m => m.SetupQueue(definition), Times.Once);
	}

    [Test]
    public void DisposeTest() {
		consumer.Consume(new Definition(), _ => { });
		mockChannel.Verify(m => m.Dispose(), Times.Never);
		
        consumer.Dispose();
		
        mockChannel.Verify(m => m.Dispose(), Times.Once);
    }

    private class Definition : DefaultDefinition<string> {
		public override string Exchange => "exchange";
		public override string QueueName => "queue";
		public override string RoutingKey => "routingKey";
	}
}
