namespace RabbitMqConnector;

internal static class LinqExtension {
	public static void Consume<T>(this IEnumerable<T> collection, Action<T> action) {
		foreach (var it in collection) {
			action(it);
		}
	}
}
