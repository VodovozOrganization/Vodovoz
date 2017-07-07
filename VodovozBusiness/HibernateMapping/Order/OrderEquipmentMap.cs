using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping
{
	public class OrderEquipmentMap : ClassMap<OrderEquipment>
	{
		public OrderEquipmentMap ()
		{
			Table ("order_equipment");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Direction).Column ("direction").CustomType<DirectionStringType> ();
			Map (x => x.Reason).Column ("reason").CustomType<ReasonStringType> ();
			Map (x => x.Confirmed).Column("confirmed");
			Map (x => x.ConfirmedComment).Column("confirmed_comments");

			References (x => x.ServiceClaim).Column("service_claim_id");
			References (x => x.Order).Column ("order_id");
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.OrderItem).Column ("order_item_id");
			References (x => x.NewEquipmentNomenclature).Column ("new_eq_nomenclature_id");
			References (x => x.CounterpartyMovementOperation).Column ("counterparty_movement_operation_id").Cascade.All();
		}
	}
}