using System;
using System.Runtime.Serialization;

namespace Vodovoz.Tools
{
	public class GeoGroupVersionNotFoundException : InvalidOperationException
	{
		public GeoGroupVersionNotFoundException()
		{
		}

		public GeoGroupVersionNotFoundException(string message) : base(message)
		{
		}

		public GeoGroupVersionNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected GeoGroupVersionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
