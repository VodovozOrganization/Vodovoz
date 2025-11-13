using System;

namespace Edo.Common
{
	public class EdoException : Exception
	{
		public EdoException()
		{
		}

		public EdoException(string message) : base(message)
		{
		}

		public EdoException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
