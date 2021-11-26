using System;

namespace WhereIsTheBottle.Infrastructure
{
	public class TerminatingException : Exception
	{
		public TerminatingException(Exception innerException) : base(null, innerException)
		{ }

		public TerminatingException(string message) : base(message)
		{ }

		public TerminatingException(string message, Exception innerException) : base(message, innerException)
		{ }
	}
}
