using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.HibernateMapping.Logistic.FastDelivery
{
	public class FastDeliveryAvailabilityHistoryMap : ClassMap<FastDeliveryAvailabilityHistory>
	{
		public FastDeliveryAvailabilityHistoryMap()
		{
			Table("fast_delivery_availability_history");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.VerificationDate).Column("verification_date").ReadOnly();
			Map(x => x.LogisticianComment).Column("logistician_comment");
			Map(x => x.LogisticianCommentVersion).Column("logistician_comment_version");
			Map(x => x.IsGetClosestByRoute).Column("is_get_closest_by_route");
			Map(x => x.MaxDistanceToLatestTrackPointKm).Column("max_distance_to_latest_track_point_km");
			Map(x => x.DriverGoodWeightLiftPerHandInKg).Column("driver_good_weight_lift_per_hand_in_kg");
			Map(x => x.MaxFastOrdersPerSpecificTime).Column("max_fast_orders_per_specific_time");
			Map(x => x.MaxTimeForFastDelivery).Column("max_time_for_fast_delivery").CustomType<TimeAsTimeSpanType>();
			Map(x => x.MinTimeForNewFastDeliveryOrder).Column("min_time_for_new_fast_delivery_order").CustomType<TimeAsTimeSpanType>();
			Map(x => x.DriverUnloadTime).Column("driver_unload_time").CustomType<TimeAsTimeSpanType>();
			Map(x => x.SpecificTimeForMaxFastOrdersCount).Column("specific_time_for_max_fast_orders_count").CustomType<TimeAsTimeSpanType>();
			Map(x => x.AddressWithoutDeliveryPoint).Column("address_without_delivery_point");

			References(x => x.Order).Column("order_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.District).Column("district_id");
			References(x => x.Logistician).Column("logistician_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Counterparty).Column("counterparty_id");

			HasMany(x => x.Items).KeyColumn("fast_delivery_availability_history_id").Cascade.AllDeleteOrphan().Inverse();
			HasMany(x => x.OrderItemsHistory).KeyColumn("fast_delivery_availability_history_id").Cascade.AllDeleteOrphan().Inverse();
			HasMany(x => x.NomenclatureDistributionHistoryItems).KeyColumn("fast_delivery_availability_history_id").Cascade.AllDeleteOrphan().Inverse();
		}
	}
}
