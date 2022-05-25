using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.HibernateMapping.Logistic.FastDelivery
{
	public class FastDelliveryAvailabilityHistoryMap:ClassMap<FastDeliveryAvailabilityHistory>
	{
		public FastDelliveryAvailabilityHistoryMap()
		{
			Table("fast_delivery_availability_history");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FastDeliveryMaxDistanceKm).Column("fast_delivery_max_distance_km");
			Map(x => x.VerificationDate).Column("verification_date").ReadOnly();
			Map(x => x.LogisticianComment).Column("logistician_comment");
			Map(x => x.LogisticianCommentVersion).Column("logistician_comment_version").ReadOnly();
			Map(x => x.LogisticianReactionTime).Column("logistician_reaction_time");
			Map(x => x.IsValid).Column("is_valid");
			Map(x => x.IsGetClosestByRoute).Column("is_get_closest_by_route");

			References(x => x.Order).Column("order_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.District).Column("district_id");
			References(x => x.Logistician).Column("logistician_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Counterparty).Column("counterparty_id");

			/*HasMany(x => x.Items).KeyColumn("fast_delivery_availability_history_items").Cascade.AllDeleteOrphan().Inverse();
			HasMany(x => x.OrderItemsHistoryItems).KeyColumn("fast_delivery_order_items_history").Cascade.AllDeleteOrphan().Inverse();
			HasMany(x => x.NomenclatureDistributionHistoryItems).KeyColumn("fast_delivery_nomenclature_distribution_history").Cascade.AllDeleteOrphan().Inverse();*/
		}
	}
}
