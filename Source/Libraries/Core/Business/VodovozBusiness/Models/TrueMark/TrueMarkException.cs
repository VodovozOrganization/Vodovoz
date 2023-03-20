using System;
using System.Runtime.Serialization;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkException : Exception
	{
		public TrueMarkException()
		{
		}

		public TrueMarkException(string message) : base(message)
		{
		}

		public TrueMarkException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TrueMarkException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
