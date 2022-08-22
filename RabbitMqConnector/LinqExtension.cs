namespace RabbitMqConnector;

internal static class LinqExtension {
	public static void Consume<T>(this IEnumerable<T> collection, Action<T> action) {
		foreach (var it in collection) {
			action(it);
		}
	}

	public static void Consume<T>(this IEnumerable<T> collection) {
		foreach (var it in collection) {
			// just iterate to consume the generator, do nothing else
		}
	}
}
