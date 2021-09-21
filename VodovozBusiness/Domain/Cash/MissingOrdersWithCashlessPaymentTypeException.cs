using System;
using System.Runtime.Serialization;

namespace Vodovoz.Domain.Cash
{
	[Serializable]
	public class MissingOrdersWithCashlessPaymentTypeException : Exception
	{
		public MissingOrdersWithCashlessPaymentTypeException()
		{
		}

		public MissingOrdersWithCashlessPaymentTypeException(string message) : base(message)
		{
		}

		public MissingOrdersWithCashlessPaymentTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MissingOrdersWithCashlessPaymentTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
