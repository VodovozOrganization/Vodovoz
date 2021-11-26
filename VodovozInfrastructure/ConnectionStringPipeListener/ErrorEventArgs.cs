using System;

namespace VodovozInfrastructure.ConnectionStringPipeListener
{
	public class ErrorEventArgs : EventArgs
	{
		public ErrorEventArgs(Exception ex, string message)
		{
			Exception = ex;
			Message = message;
		}

		public Exception Exception { get; }
		public string Message { get; }
	}
}