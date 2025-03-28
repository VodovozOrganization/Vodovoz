using System;
using Edo.Problems.Exception;

namespace TrueMark.Codes.Pool
{
	public class EdoCodePoolMissingCodeException : EdoProblemException
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
