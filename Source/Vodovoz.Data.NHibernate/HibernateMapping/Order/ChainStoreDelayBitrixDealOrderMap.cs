using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class ChainStoreDelayBitrixDealOrderMap : ClassMap<ChainStoreDelayBitrixDealOrder>
	{
		public ChainStoreDelayBitrixDealOrderMap()
		{
			Table("chain_store_delay_bitrix_deal_orders");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Deal).Column("chain_store_delay_bitrix_deal_id");

			Map(x => x.OrderId).Column("order_id");
			Map(x => x.DeliveryPointId).Column("delivery_point_id");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");
			Map(x => x.EstimatedPaymentDate).Column("estimated_payment_date");
			Map(x => x.OrderSum).Column("order_sum");
			Map(x => x.Debt).Column("debt");
		}
	}
}
