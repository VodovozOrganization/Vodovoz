using System;
using System.Runtime.Serialization;

namespace EmailService.Mailjet
{
	[DataContract]
	public class MailjetErrorInfo
	{
		[DataMember]
		public string ErrorIdentifier { get; set; }

		[DataMember]
		public string ErrorCode { get; set; }

		[DataMember]
		public int StatusCode { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public string[] ErrorRelatedTo { get; set; }
	}
}
