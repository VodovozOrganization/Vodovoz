using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class MobileAppCounterpartyMap : SubclassMap<MobileAppCounterpartyEntity>
	{
		public MobileAppCounterpartyMap()
		{
			DiscriminatorValue(nameof(CounterpartyFrom.MobileApp));
		}
	}
}
