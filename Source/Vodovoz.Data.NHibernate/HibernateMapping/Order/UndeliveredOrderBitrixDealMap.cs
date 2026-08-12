using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveredOrderBitrixDealMap : ClassMap<UndeliveredOrderBitrixDeal>
	{
		public UndeliveredOrderBitrixDealMap()
		{
			Table("undelivered_order_bitrix_deals");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.LastUpdateDate).Column("last_update_date");
			Map(x => x.LastSynchronizedDate).Column("last_synchronized_date");
			Map(x => x.Status).Column("status");
			Map(x => x.UndeliveredOrderId).Column("undelivered_order_id");
			Map(x => x.BitrixDealId).Column("bitrix_deal_id");
			Map(x => x.SourceLastEditedTime).Column("source_last_edited_time");
			Map(x => x.DealTitle).Column("deal_title");
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.CounterpartyName).Column("counterparty_name");
			Map(x => x.DeliveryAddress).Column("delivery_address");
			Map(x => x.DeliveryDate).Column("delivery_date");
			Map(x => x.DeliveryInterval).Column("delivery_interval");
			Map(x => x.DriverName).Column("driver_name");
			Map(x => x.RouteListId).Column("route_list_id");
			Map(x => x.TypeOfUndelivery).Column("type_of_undelivery");
			Map(x => x.SpecificationOfUndelivery).Column("specification_of_undelivery");
			Map(x => x.Comment).Column("comment");
			Map(x => x.NewOrderId).Column("new_order_id");
			Map(x => x.LastError).Column("last_error");
		}
	}
}
