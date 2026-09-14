using System.Collections.Generic;

namespace Edo.Problems.Exception
{
	public class MassTransitExceptionInfo
	{
		/// <summary>
		/// The type name of the exception
		/// </summary>
		public string ExceptionType { get; set; }

		/// <summary>
		/// The inner exception if present (also converted to ExceptionInfo)
		/// </summary>
		public MassTransitExceptionInfo InnerException { get; set; }

		/// <summary>
		/// The stack trace of the exception site
		/// </summary>
		public string StackTrace { get; set; }

		/// <summary>
		/// The exception message
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The exception source
		/// </summary>
		public string Source { get; set; }

		public IDictionary<string, object> Data { get; set; }
	}
}
