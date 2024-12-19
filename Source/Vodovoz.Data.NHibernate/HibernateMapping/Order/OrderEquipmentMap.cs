using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OrderEquipmentMap : ClassMap<OrderEquipment>
	{
		public OrderEquipmentMap()
		{
			Table("order_equipment");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Direction).Column("direction");
			Map(x => x.DirectionReason).Column("direction_reason");
			Map(x => x.Reason).Column("reason");
			Map(x => x.Confirmed).Column("confirmed");
			Map(x => x.ConfirmedComment).Column("confirmed_comments");
			Map(x => x.OwnType).Column("own_type");
			Map(x => x.Count).Column("count");
			Map(x => x.ActualCount).Column("actual_count");

			References(x => x.ServiceClaim).Column("service_claim_id");
			References(x => x.Order).Column("order_id");
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.OrderItem).Column("order_item_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.CounterpartyMovementOperation).Column("counterparty_movement_operation_id").Cascade.All();
			References(x => x.OrderRentDepositItem).Column("order_rent_deposit_item_id");
			References(x => x.OrderRentServiceItem).Column("order_rent_service_item_id");
		}
	}
}
