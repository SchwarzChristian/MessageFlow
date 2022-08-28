namespace MessageFlow.Entities;

/// <summary>
/// An error catched during message processing.
/// </summary>
public class Error<T> {
	/// <summary>
	/// message that caused the error
	/// </summary>
	public Message<T>? Message { get; set; }

	/// <summary>
	/// problem description
	/// </summary>
	public string Problem { get; set; } = "<unknown>";

	/// <summary>
	/// type of the catched exception
	/// </summary>
	public string Type { get; set; } = "<unknown>";

	/// <summary>
	/// stack trace of the catched exception
	/// </summary>
	public ICollection<string> StackTrace { get; set; } = new List<string>();

	/// <summary>
	/// when the error occured
	/// </summary>
	public DateTime CreatedAt { get; set; } = DateTime.Now;
}
