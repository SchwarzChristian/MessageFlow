using FluentAssertions;
using RabbitMqConnector.Tests.Helper;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Tests;

public class WorkflowTests {
    [Test]
    public void DefineTest() {
        var workflow = new TestWorkflowBuilder().BuildWorkflow();

        workflow.Steps.Count.Should().Be(3);
        workflow.Steps.ElementAt(0).Should().BeOfType<Step1>();
        workflow.Steps.ElementAt(1).Should().BeOfType<Step2>();
        workflow.Steps.ElementAt(2).Should().BeOfType<Step3>();
    }

    [Test]
    public void NamedWorkflowTest() {
        var workflow = new TestWorkflowBuilder().BuildWorkflow();
        
        workflow.AddNamedWorkflow("name", workflow);

        var steps = new IWorkerDefinition[] {
            new Step1(),
            new Step2(),
            new Step3(),
        };

        workflow.NamedWorkflows.Should().HaveCount(1);
        var namedWorkflow = workflow.NamedWorkflows.First();
        namedWorkflow.Name.Should().Be("name");
        namedWorkflow.Workflow.Select(s => s.Exchange)
            .Should().ContainInOrder(steps.Select(s => s.Exchange));
        namedWorkflow.Workflow.Select(s => s.RoutingKey)
            .Should().ContainInOrder(steps.Select(s => s.RoutingKey));
    }
}