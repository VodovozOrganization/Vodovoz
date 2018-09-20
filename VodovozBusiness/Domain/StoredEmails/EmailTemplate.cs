using System;
using System.Collections.Generic;
using QSOrmProject;

namespace Vodovoz.Domain.StoredEmails
{
	public class EmailTemplate
	{
		public virtual string Title { get; set; }
		public virtual string Text { get; set; }
		public virtual string TextHtml { get; set; }
		public virtual Dictionary<string, EmailAttachment> Attachments { get; set; } = new Dictionary<string, EmailAttachment>();
	}
}
