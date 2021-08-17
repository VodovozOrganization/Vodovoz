using System.Collections.Generic;

namespace Mailjet.Api.Abstractions
{
	public class MessageSendDetails
	{
		public string Status { get; set; } //success, error
		public ICollection<SendDetails> To { get; set; }
		public ICollection<SendDetails> Cc { get; set; }
		public ICollection<SendDetails> Bcc { get; set; }
		public ICollection<ErrorDetails> Errors { get; set; }
	}
}
