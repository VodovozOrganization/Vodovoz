namespace Edo.Common
{
	public class CodeDuplicatedException : System.Exception
	{
		public CodeDuplicatedException(string message)
		{
			Message = message ?? throw new System.ArgumentNullException(nameof(message));
		}

		public override string Message { get; }
	}
}
