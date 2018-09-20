using System;
namespace Vodovoz.Domain.StoredEmails
{
	public class EmailAttachment
	{
		public string MIMEType { get; set; }
		public string FileName { get; set; }
		public string Base64Content { get; set; }
	}
}
