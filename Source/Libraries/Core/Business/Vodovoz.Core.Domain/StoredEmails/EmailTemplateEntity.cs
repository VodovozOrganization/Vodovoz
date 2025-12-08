using System.Collections.Generic;

namespace Vodovoz.Core.Domain.StoredEmails
{
	public class EmailTemplateEntity
	{
		public virtual string Title { get; set; }
		public virtual string Text { get; set; }
		public virtual string TextHtml { get; set; }
		public virtual Dictionary<string, EmailAttachment> Attachments { get; set; } = new Dictionary<string, EmailAttachment>();
	}
}
