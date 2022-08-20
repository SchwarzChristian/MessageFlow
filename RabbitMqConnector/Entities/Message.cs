namespace RabbitMqConnector.Entities;

public class Message<T> {
	public string Environment { get; set; } = null!;
	public string? Project { get; set; }
	public string Action { get; set; } = null!;
	public T Content { get; set; } = default!;
}
