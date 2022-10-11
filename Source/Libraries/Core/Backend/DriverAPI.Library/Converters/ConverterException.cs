using System;
using System.Runtime.Serialization;

namespace DriverAPI.Library.Converters
{
	public class ConverterException : ArgumentOutOfRangeException
	{
		public ConverterException()
		{
		}

		public ConverterException(string paramName) : base(paramName)
		{
		}

		public ConverterException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ConverterException(string paramName, string message) : base(paramName, message)
		{
		}

		public ConverterException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
		{
		}

		protected ConverterException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
