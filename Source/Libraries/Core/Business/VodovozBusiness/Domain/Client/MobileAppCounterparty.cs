using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Domain.Client
{
	public class MobileAppCounterparty : ExternalCounterparty
	{
		public override CounterpartyFrom CounterpartyFrom => CounterpartyFrom.MobileApp;
	}
}
