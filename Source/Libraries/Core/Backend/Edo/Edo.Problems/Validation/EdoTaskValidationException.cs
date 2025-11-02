namespace Edo.Problems.Validation
{
	public class EdoTaskValidationException : System.Exception
	{
		public EdoTaskValidationException(string message) : base(message)
		{
		}

		public EdoTaskValidationException(string message, System.Exception innerException) : base(message, innerException)
		{
		}
	}
}
