using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class WebSiteCounterpartyMap : SubclassMap<WebSiteCounterpartyEntity>
	{
		public WebSiteCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.WebSite));
		}
	}
}
