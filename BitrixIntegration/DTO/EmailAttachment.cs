using System.Runtime.Serialization;

namespace BitrixIntegration.DTO
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
