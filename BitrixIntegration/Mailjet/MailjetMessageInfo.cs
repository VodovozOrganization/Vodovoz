using System;
using System.Runtime.Serialization;

namespace EmailService.Mailjet
{
	[DataContract]
	public class MailjetMessageInfo
	{
		[DataMember]
		public string Email { get; set; }

		[DataMember]
		public string MessageUUID { get; set; }

		[DataMember]
		public long MessageID { get; set; }

		[DataMember]
		public string MessageHref { get; set; }
	}
}
