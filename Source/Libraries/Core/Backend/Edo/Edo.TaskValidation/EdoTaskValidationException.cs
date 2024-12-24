using System;

namespace Edo.TaskValidation
{
	public class EdoTaskValidationException : Exception
	{
		public EdoTaskValidationException(string message) : base(message)
		{
		}

		public EdoTaskValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
