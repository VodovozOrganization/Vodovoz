using Vodovoz.Core.Domain.Edo;

namespace Edo.Contracts.Messages.Events
{
	public class TransferCompleteEvent
	{
		public int TransferIterationId { get; set; }
		public TransferInitiator TransferInitiator { get; set; }
	}
}
