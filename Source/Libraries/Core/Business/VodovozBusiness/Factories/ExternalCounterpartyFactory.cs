using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Factories
{
	public class ExternalCounterpartyFactory : IExternalCounterpartyFactory
	{
		public ExternalCounterparty CreateNewExternalCounterparty(CounterpartyFrom counterpartyFrom)
		{
			switch(counterpartyFrom)
			{
				case CounterpartyFrom.MobileApp:
					return new MobileAppCounterparty();
				case CounterpartyFrom.WebSite:
					return new WebSiteCounterparty();
				default:
					throw new ArgumentOutOfRangeException(nameof(counterpartyFrom), counterpartyFrom, null);
			}
		}
	}
}
