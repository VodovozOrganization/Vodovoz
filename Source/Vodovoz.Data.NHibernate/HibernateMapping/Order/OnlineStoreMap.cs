using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class OnlineStoreMap : ClassMap<OnlineStore>
	{
		public OnlineStoreMap()
		{
			Table("online_stores");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}

	}
}