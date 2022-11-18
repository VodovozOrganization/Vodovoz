using System.Collections.Generic;

namespace Mailjet.Api.Abstractions
{
	public class MessageSendDetails
	{
		public string Status { get; set; } //success, error
		public ICollection<SendDetails> To { get; set; }
	}
}
