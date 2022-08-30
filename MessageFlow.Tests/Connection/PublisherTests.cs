using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Tests.Helper;
using MessageFlow.Workflow;

namespace MessageFlow.Tests.Connection;

public class PublisherTests {
	private Mock<IModel> mockChannel = null!;
	private Mock<IConnector> mockConnector = null!;
	private Publisher publisher = null!;

	[SetUp]
    public void Setup() {
        mockChannel = new Mock<IModel>();
        mockConnector = new Mock<IConnector>();
        mockConnector.Setup(m => m.OpenChannel()).Returns(mockChannel.Object);
		mockConnector.Setup(m => m.ErrorExchangeName).Returns("errors");
		mockConnector.Setup(m => m.Config).Returns(new RabbitMqConfig());
		publisher = new Publisher(mockConnector.Object);
    }

    [Test]
    public void PublishTest() {
        var workflow = new TestWorkflowBuilder().BuildWorkflow();
        mockChannel.Setup(m => m.BasicPublish(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()
        )).Callback(CheckPublishedMessage);

        publisher.Publish(workflow, "message");

        mockChannel.Verify(m => m.BasicPublish(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()
        ), Times.Once);
    }

    private void CheckPublishedMessage(
        string exchange,
        string routingKey,
        bool mandatory,
        IBasicProperties props,
        ReadOnlyMemory<byte> data
    ) {
        var decoded = Encoding.UTF8.GetString(data.ToArray());
        var message = JsonConvert.DeserializeObject<Message<string>>(decoded);

        exchange.Should().Be("prod." + new Step1().Exchange);
        routingKey.Should().Be(new Step1().RoutingKey);
        message?.Should().NotBeNull();
        message?.Content.Should().Be("message");
        message?.CurrentStep.Exchange.Should().Be(exchange);
        message?.CurrentStep.RoutingKey.Should().Be(routingKey);
        message?.History.Should().BeEmpty();
        message?.PendingSteps.Should().HaveCount(2);
        message?.PendingSteps.Select(step => step.RoutingKey).Should().ContainInOrder(
            new Step2().RoutingKey,
            new Step3().RoutingKey
        );
    }

    [Test]
    public void PublishErrorTest() {
		var message = new Message<string> {
			Content = "content",
		};

		var exception = new NotImplementedException();

        mockChannel.Setup(m => m.BasicPublish(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()
        )).Callback(CheckErrorMessage);

		publisher.PublishError(message, exception);

	}
	private void CheckErrorMessage(
		string exchange,
		string routingKey,
		bool mandatory,
		IBasicProperties props,
		ReadOnlyMemory<byte> data
	) {
        var decoded = Encoding.UTF8.GetString(data.ToArray());
        var errorMessage = JsonConvert.DeserializeObject<Error<string>>(decoded);
		var expectedException = new NotImplementedException();

		errorMessage.Should().NotBeNull();
		errorMessage!.Message.Should().NotBeNull();
		errorMessage!.Message?.Content.Should().Be("content");
		errorMessage!.Problem.Should().Be(expectedException.Message);
		errorMessage!.Type.Should().Be(nameof(NotImplementedException));
	}
}
