using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.HibernateMapping.Logistic.FastDelivery
{
	public class FastDeliveryAvailabilityHistoryItemMap: ClassMap<FastDeliveryAvailabilityHistoryItem>
	{
		public FastDeliveryAvailabilityHistoryItemMap()
		{
			Table("fast_delivery_availability_history_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DistanceByLineToClient).Column("distance_by_line_to_client");
			Map(x => x.IsValidDistanceByLineToClient).Column("is_valid_distance_by_line_to_client");
			Map(x => x.DistanceByRoadToClient).Column("distance_by_road_to_client");
			Map(x => x.IsValidDistanceByRoadToClient).Column("is_valid_distance_by_road_to_client");
			Map(x => x.IsGoodsEnough).Column("is_goods_enough");
			Map(x => x.IsValidIsGoodsEnough).Column("is_valid_is_goods_enough");
			Map(x => x.UnclosedFastDeliveries).Column("unclosed_fast_deliveries");
			Map(x => x.IsValidUnclosedFastDeliveries).Column("is_valid_unclosed_fast_deliveries");
			Map(x => x.RemainingTimeForShipmentNewOrder).Column("remaining_time_for_shipment_new_order").CustomType<TimeAsTimeSpanType>();
			Map(x => x.IsValidRemainingTimeForShipmentNewOrder).Column("is_valid_remaining_time_for_shipment_new_order");
			Map(x => x.LastCoordinateTimeElapsed).Column("last_coordinate_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.IsValidLastCoordinateTime).Column("is_valid_last_coordinate_time");
			Map(x => x.IsValidToFastDelivery).Column("is_valid_to_fast_delivery");

			References(x => x.RouteList).Column("route_list_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.FastDeliveryAvailabilityHistory).Column("fast_delivery_availability_history_id");
		}
	}
}
