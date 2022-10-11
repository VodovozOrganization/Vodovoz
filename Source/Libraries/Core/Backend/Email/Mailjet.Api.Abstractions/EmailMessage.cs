using System.Collections.Generic;

namespace Mailjet.Api.Abstractions
{
	public class EmailMessage
	{
		public EmailContact From { get; set; }
		public ICollection<EmailContact> To { get; set; }
		public string Subject { get; set; }
		public string TextPart { get; set; }
		public string HTMLPart { get; set; }
		public ICollection<EmailAttachment> Attachments { get; set; }
		public ICollection<InlinedEmailAttachment> InlinedAttachments { get; set; }
		public string CustomId { get; set; }
		public string EventPayload { get; set; }
		public Dictionary<string, string> Headers { get; set; }
	}
}
