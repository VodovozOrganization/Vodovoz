using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	/// <summary>
	/// Маппинг класса <see cref="WebSiteCounterparty"/>
	/// </summary>
	public class WebSiteCounterpartyMap : SubclassMap<WebSiteCounterparty>
	{
		public WebSiteCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.WebSite));
		}
	}
}
