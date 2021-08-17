using System.Collections.Generic;

namespace Mailjet.Api.Abstractions
{
	public class SendPayload
	{
		public ICollection<EmailMessage> Messages { get; set; }
		public bool SandboxMode { get; set; }
	}
}
