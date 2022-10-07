using System;
using System.Runtime.Serialization;

namespace InstantSmsService
{
	[DataContract]
	public class InstantSmsMessage
	{
		[DataMember]
		public string ServerMessageId { get; set; }

		[DataMember]
		public string MobilePhone { get; set; }

		[DataMember]
		public string MessageText { get; set; }

		[DataMember]
		public DateTime? ExpiredTime { get; set; }
	}
}
