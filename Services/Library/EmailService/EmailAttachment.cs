using System.Runtime.Serialization;

namespace EmailService
{
	[DataContract]
	public class EmailAttachment
	{
		[DataMember]
		public string ContentType { get; set; }
		[DataMember]
		public string FileName { get; set; }
		[DataMember]
		public string Base64String { get; set; }
	}
}
