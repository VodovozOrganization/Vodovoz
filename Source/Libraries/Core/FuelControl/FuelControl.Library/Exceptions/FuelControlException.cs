using System;
using System.Runtime.Serialization;

namespace FuelControl.Library.Exceptions
{
	public class FuelControlException : Exception
	{
		public FuelControlException()
		{
		}

		public FuelControlException(string message) : base(message)
		{
		}

		public FuelControlException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FuelControlException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
