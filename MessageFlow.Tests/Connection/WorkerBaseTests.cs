using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageFlow.Connection;
using MessageFlow.Entities;
using MessageFlow.Exceptions;
using MessageFlow.Workflow;

namespace MessageFlow.Tests.Connection;

public class WorkerBaseTests
{
	private Worker worker = null!;
	private Mock<IConnector> mockConnector = null!;
	private Mock<IModel> mockChannel = null!;
	private readonly Message<string> testMessage = new Message<string> {
        Content = "content",
        CurrentStep = new WorkflowStep {
            Config = "23.42",
            Exchange = "exchange",
            RoutingKey = "routing.key",
        },
        History = Array.Empty<WorkflowStep>(),
        NamedWorkflows = new [] { new NamedWorkflow { 
            Name = "namedWorkflow",
            Workflow = new [] {
                new WorkflowStep { 
                    Exchange = "nwExchange.step.1",
                    RoutingKey = "nwRoutingKey.step.1",
                    Config = "nwConfig.step.1",
                },
                new WorkflowStep { 
                    Exchange = "nwExchange.step.2",
                    RoutingKey = "nwRoutingKey.step.2",
                    Config = "nwConfig.step.2",
                },
            }
        }},
        PendingSteps = new [] {
            new WorkflowStep {
                Exchange = "step.1.exchange",
                RoutingKey = "step.1.routing.key",
                Config = "step.1.config",
            },
            new WorkflowStep {
                Exchange = "step.2.exchange",
                RoutingKey = "step.2.routing.key",
                Config = "step.2.config",
            },
        },
    };

	[SetUp]
    public void Setup() {
		mockConnector = new Mock<IConnector>();
		mockChannel = new Mock<IModel>();
		mockConnector.Setup(m => m.OpenChannel()).Returns(mockChannel.Object);
		worker = new Worker();
		worker.Run(mockConnector.Object);
	}

    [Test]
    public void HandleMessageTest() {
		var args = new BasicDeliverEventArgs {
			DeliveryTag = 80,
            Body = SerializeMessage(testMessage),
		};

		mockConnector.Setup(m => m.Publish(It.IsAny<Message<int>>())).Callback(CheckMessage);

		worker.HandleNewMessage(null, args);

		mockConnector.Verify(m => m.Publish(It.IsAny<Message<int>>()), Times.Once);
		mockChannel.Verify(m => m.BasicAck(80, It.IsAny<bool>()), Times.Once);
	}

    private void CheckMessage(Message<int> message) {
		message.Should().NotBeNull();
		var expectedOutputContent = testMessage.Content.Length;
        expectedOutputContent += testMessage.CurrentStep.Config!.Length;
		message.Content.Should().Be(expectedOutputContent);
		CheckWorkflowStep(message.CurrentStep, testMessage.PendingSteps.First());
		message.History.Should().HaveCount(1);
		CheckWorkflowStep(message.History.First(), testMessage.CurrentStep);
		message.NamedWorkflows.Should().HaveCount(1);
		message.NamedWorkflows.First().Name.Should().Be(testMessage.NamedWorkflows.First().Name);
		message.NamedWorkflows.First().Workflow.Should().HaveCount(2);
		CheckWorkflowStep(
			message.NamedWorkflows.First().Workflow.First(),
			testMessage.NamedWorkflows.First().Workflow.First()
		);
		message.PendingSteps.Should().HaveCount(1);
		CheckWorkflowStep(message.PendingSteps.First(), testMessage.PendingSteps.Last());
	}

    [Test]
    [NonParallelizable]
    public void HandleMessageErrorHandlingTest() {
		testMessage.CurrentStep.Config = "42.23";
		var args = new BasicDeliverEventArgs {
			DeliveryTag = 80,
            Body = SerializeMessage(testMessage),
		};

        mockConnector.Setup(m => m.PublishError(
            It.IsAny<Message<string>>(),
            It.IsAny<Exception>()
        )).Callback(CheckError);

		worker.HandleNewMessage(null, args);

		mockConnector.Verify(m => m.PublishError(
			It.IsAny<Message<string>>(),
			It.IsAny<Exception>()
		), Times.Once);

		mockChannel.Verify(m => m.BasicAck(80, It.IsAny<bool>()), Times.Never);
		mockChannel.Verify(m => m.BasicNack(
            It.IsAny<ulong>(),
            It.IsAny<bool>(),
            It.IsAny<bool>()
        ), Times.Once);
	}

    private void CheckError(Message<string> message, Exception ex) {
		ex.Should().BeOfType<RecoverableException>();
		message.Content.Should().Be(testMessage.Content);
	}

    [Test]
    public void BranchWorkflowTest() {
		testMessage.CurrentStep.Config = "5";
		var args = new BasicDeliverEventArgs {
			DeliveryTag = 80,
            Body = SerializeMessage(testMessage),
		};

        mockConnector.Setup(m => m.Publish(
            It.IsAny<Message<string>>()
        )).Callback(CheckBranchedMessage);

		worker.HandleNewMessage(null, args);

		mockConnector.Verify(m => m.Publish(
			It.IsAny<Message<string>>()
		), Times.Once);
		mockChannel.Verify(m => m.BasicAck(80, It.IsAny<bool>()), Times.Once);
    }

    private void CheckBranchedMessage(Message<string> message) {
		message.Should().NotBeNull();
		message.Content.Should().Be("branchInput");
		CheckWorkflowStep(message.CurrentStep, testMessage.NamedWorkflows.First().Workflow.First());
		message.History.Should().HaveCount(1);
		CheckWorkflowStep(message.History.First(), testMessage.CurrentStep);
		message.NamedWorkflows.Should().HaveCount(1);
		message.NamedWorkflows.First().Name.Should().Be(testMessage.NamedWorkflows.First().Name);
		message.NamedWorkflows.First().Workflow.Should().HaveCount(1);
		CheckWorkflowStep(
			message.NamedWorkflows.First().Workflow.First(),
			testMessage.NamedWorkflows.First().Workflow.First()
		);
		message.PendingSteps.Should().HaveCount(0);
		CheckWorkflowStep(
            message.PendingSteps.First(),
            testMessage.NamedWorkflows.First().Workflow.Last()
        );
	}

    private byte[] SerializeMessage(Message<string> message) {
		var serialized = JsonConvert.SerializeObject(message);
		var binary = Encoding.UTF8.GetBytes(serialized);
		return binary;
	}

    private void CheckWorkflowStep(WorkflowStep input, WorkflowStep output) {
		output.Config.Should().Be(input.Config);
		output.Exchange.Should().Be(input.Exchange);
		output.RoutingKey.Should().Be(input.RoutingKey);
    }

    private class WorkerDefinition : DefaultDefinition<string, int, float> {}

	private class Worker : WorkerBase<string, int, float> {
		public override IWorkerDefinition<string, int, float> Definition =>
            new WorkerDefinition();

		public override IEnumerable<int> Process(string input, float config) {
            if (config > 30) throw new RecoverableException("problem", new Exception());
            if (config < 10) BranchWorkflow("namedWorkflow", "branchInput");
			return new[] { input.Length + config.ToString().Length };
		}
	}
}
