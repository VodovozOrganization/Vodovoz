using System;

namespace DriverAPI.Library.Helpers
{
	public class SmsPaymentServiceAPIHelperException : Exception
	{
		public SmsPaymentServiceAPIHelperException(string message) : base(message) { }
	}
}
