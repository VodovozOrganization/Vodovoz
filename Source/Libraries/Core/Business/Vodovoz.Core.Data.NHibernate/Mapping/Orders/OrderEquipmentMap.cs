using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OrderEquipmentMap : ClassMap<OrderEquipmentEntity>
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

			References(x => x.Order).Column("order_id");
			References(x => x.Equipment).Column("equipment_id");
			References(x => x.OrderItem).Column("order_item_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
