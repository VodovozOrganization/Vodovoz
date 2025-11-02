using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories
{
	public class GuidCounterpartyEmailNode
	{
		public int CounterpartyId { get; set; }
		public BulkEmailEvent.BulkEmailEventType? BulkEmailEventType { get; set; }
	}
}
