using System;

namespace TrueMark.Codes.Pool
{
	public class EdoCodePoolMissingCodeException : Exception
	{
		public EdoCodePoolMissingCodeException()
		{
		}

		public EdoCodePoolMissingCodeException(string message) : base(message)
		{
		}

		public EdoCodePoolMissingCodeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
