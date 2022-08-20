using FluentAssertions;
using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Tests;

public class WorkflowTests {
    [Test]
    public void DefineTest() {
        var workflow = new Workflow<string>();
        workflow.Define()
            .Step<Step1, int>()
            .Step<Step2, bool, float>(5f)
            .Step<Step3>();

        workflow.Steps.Count.Should().Be(3);
        workflow.Steps.ElementAt(0).Should().BeOfType<Step1>();
        workflow.Steps.ElementAt(1).Should().BeOfType<Step2>();
        workflow.Steps.ElementAt(2).Should().BeOfType<Step3>();
    }

    private class Step1 : DefaultDefinition<string, int> {}
    private class Step2 : DefaultDefinition<int, bool, float> {}
    private class Step3 : DefaultDefinition<bool> {}
}