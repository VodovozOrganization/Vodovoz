using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.HibernateMapping.Logistic.FastDelivery
{
	public class FastDeliveryOrderItemsHistoryMap : ClassMap<FastDeliveryOrderItemHistory>
	{
		public FastDeliveryOrderItemsHistoryMap()
		{
			Table("fast_delivery_order_items_history");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.FastDeliveryAvailabilityHistory).Column("fast_delivery_availability_history_id");
		}
	}
}
