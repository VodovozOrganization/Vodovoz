using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class WebSiteCounterpartyMap : SubclassMap<WebSiteCounterparty>
	{
		public WebSiteCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.WebSite));
		}
	}
}
