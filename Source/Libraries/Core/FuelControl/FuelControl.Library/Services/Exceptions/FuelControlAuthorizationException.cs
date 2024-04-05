using System.Runtime.Serialization;
using System;

namespace FuelControl.Library.Services.Exceptions
{
	public class FuelControlAuthorizationException : FuelControlException
	{
		public FuelControlAuthorizationException()
		{
		}

		public FuelControlAuthorizationException(string message) : base(message)
		{
		}

		public FuelControlAuthorizationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FuelControlAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
