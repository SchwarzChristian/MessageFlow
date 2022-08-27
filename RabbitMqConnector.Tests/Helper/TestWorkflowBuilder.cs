using RabbitMqConnector.Entities;
using RabbitMqConnector.Workflow;

namespace RabbitMqConnector.Tests.Helper;

public class TestWorkflowBuilder {
	internal Workflow<string> BuildWorkflow() {
		var workflow = new Workflow<string>();
        workflow.Define()
            .Step<Step1, int>()
            .Step<Step2, bool, float>(5f)
            .Step<Step3>();

		return workflow;
	}
}

internal class Step1 : DefaultDefinition<string, int> {}
internal class Step2 : DefaultDefinition<int, bool, float> {}
internal class Step3 : DefaultDefinition<bool> {}