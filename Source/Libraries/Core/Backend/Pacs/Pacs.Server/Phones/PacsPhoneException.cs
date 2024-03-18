using System;
using System.Runtime.Serialization;

namespace Pacs.Server.Phones
{
	public class PacsPhoneException : Exception
	{
		public PacsPhoneException()
		{
		}

		public PacsPhoneException(string message) : base(message)
		{
		}

		public PacsPhoneException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PacsPhoneException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
