using FluentAssertions;

namespace RabbitMqConnector.Tests;

public class LinqExtensionTests
{
    [Test]
    public void ConsumeWithAtionTest() {
        var input = new [] { 5, 23, 42 };
        var fromSelect = new List<int>();
        var fromConsume = new List<int>();

        input
            .Select(it => { fromSelect.Add(it); return it; })
            .Consume(it => fromConsume.Add(it));

        fromSelect.Should().ContainInOrder(input);
        fromConsume.Should().ContainInOrder(input);
    }

    [Test]
    public void ConsumeWithoutActionTest() {
        var input = new [] { 5, 23, 42 };
        var fromSelect = new List<int>();

        input.Select(it => { fromSelect.Add(it); return it; }).Consume();

        fromSelect.Should().ContainInOrder(input);
    }
}
