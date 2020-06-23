using System;
using System.Runtime.Serialization;

namespace EmailService.Mailjet
{
	[DataContract]
	public class MailjetMessage
	{
		[DataMember]
		public MailjetErrorInfo[] Errors { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public string CustomID { get; set; }

		[DataMember]
		public MailjetMessageInfo[] To { get; set; }
		[DataMember]
		public MailjetMessageInfo[] Cc { get; set; }
		[DataMember]
		public MailjetMessageInfo[] Bcc { get; set; }
	}
}
