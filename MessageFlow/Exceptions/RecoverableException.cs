namespace MessageFlow.Exceptions;

[System.Serializable]
public class RecoverableException : System.Exception
{
	public RecoverableException() { }
	public RecoverableException(string message) : base(message) { }
	public RecoverableException(string message, System.Exception inner) : base(message, inner) { }
	protected RecoverableException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
