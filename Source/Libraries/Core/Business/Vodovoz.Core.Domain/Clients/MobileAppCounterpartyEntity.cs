namespace Vodovoz.Core.Domain.Clients
{
	public class MobileAppCounterpartyEntity : ExternalCounterpartyEntity
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.MobileApp;
	}
}
