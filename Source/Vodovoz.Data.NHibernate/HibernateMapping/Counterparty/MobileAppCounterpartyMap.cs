using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class MobileAppCounterpartyMap : SubclassMap<MobileAppCounterparty>
	{
		public MobileAppCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.MobileApp));
		}
	}
}
