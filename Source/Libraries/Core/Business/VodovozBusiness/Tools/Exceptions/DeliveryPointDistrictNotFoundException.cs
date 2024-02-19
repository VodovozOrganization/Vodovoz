using System;
using System.Runtime.Serialization;

namespace Vodovoz.Tools.Exceptions
{
	public class DeliveryPointDistrictNotFoundException : InvalidOperationException
	{
		public DeliveryPointDistrictNotFoundException()
		{
		}

		public DeliveryPointDistrictNotFoundException(string message) : base(message)
		{
		}

		public DeliveryPointDistrictNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DeliveryPointDistrictNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
