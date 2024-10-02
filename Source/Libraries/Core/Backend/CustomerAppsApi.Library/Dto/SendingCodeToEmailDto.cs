using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto
{
	public class SendingCodeToEmailDto
	{
		public Source Source { get; set; }
		public Guid ExternalUserId { get; set; }
		public int CounterpartyId { get; set; }
		public string EmailAddress { get; set; }
		public string Message { get; set; }
	}
}
