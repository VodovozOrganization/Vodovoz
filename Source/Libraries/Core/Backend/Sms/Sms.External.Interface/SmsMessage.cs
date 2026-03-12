using System;

namespace Sms.External.Interface
{
	public class SmsMessage : ISmsMessage
	{
		public string MobilePhoneNumber { get; set; }

		public string LocalId { get; set; }

		public DateTime ScheduleTime { get; set; }

		public string MessageText { get; set; }

		public SmsMessage(string phone, string localId, string messageText)
		{
			MobilePhoneNumber = phone;
			LocalId = localId;
			MessageText = messageText;
		}

		public SmsMessage(string phone, string localId, string messageText, DateTime scheduleTime) : this(phone, localId, messageText)
		{
			ScheduleTime = scheduleTime;
		}

		public static ISmsMessage Create(string mobilePhoneNumber, string localId, string messageText)
		{
			return new SmsMessage(mobilePhoneNumber, localId, messageText);
		}
	}
}
