using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkAddressMap : ComponentMap<BulkAddress>
	{
		public BulkAddressMap()
		{
			Map(x => x.City).Column("address_city");
			Map(x => x.Street).Column("address_street");
		}
	}
}
