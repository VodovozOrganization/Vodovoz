using System;
using System.Runtime.Serialization;

namespace DriverAPI.Library.Exceptions
{
	public class DataNotFoundException : ArgumentOutOfRangeException
	{
		public DataNotFoundException()
		{
		}

		public DataNotFoundException(string paramName) : base(paramName)
		{
		}

		public DataNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public DataNotFoundException(string paramName, string message) : base(paramName, message)
		{
		}

		public DataNotFoundException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
		{
		}

		protected DataNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
