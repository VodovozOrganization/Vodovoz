using System.Collections.Generic;

namespace Mailjet.Api.Abstractions
{
	public class SendResponse
	{
		public ICollection<MessageSendDetails> Messages { get; set; }
	}
}
