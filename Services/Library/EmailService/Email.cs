using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EmailService
{
	[DataContract]
	public class Email
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Text { get; set; }

		/// <summary>
		/// Список файлов в виде словаря, где ключ - имя файла, значение - строка закодированная в base64
		/// </summary>
		[DataMember]
		public Dictionary<string, string> AttachmentsBinary { get; set; }

		[DataMember]
		public Dictionary<string, EmailAttachment> InlinedAttachments { get; set; }

		[DataMember]
		public string HtmlText { get; set; }

		[DataMember]
		public EmailContact Sender { get; set; }

		[DataMember]
		public EmailContact Recipient { get; set; }

		public int StoredEmailId { get; set; }
	}
}
