using System.Linq;

namespace Sms.External.Interface
{
	public static class SmsResponseStatusHelper
	{
		public static bool IsSuccefullStatus(this SmsResponseStatus status) =>
			new[]
			{
				SmsResponseStatus.MessageAccepted,
				SmsResponseStatus.MessageTransmittedToTheOperator,
				SmsResponseStatus.MessageOnThewWay,
				SmsResponseStatus.MessageDelivered
			}.Contains(status);
	}
}
