using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class ChainStoreDelayBitrixDealMap : ClassMap<ChainStoreDelayBitrixDeal>
	{
		public ChainStoreDelayBitrixDealMap()
		{
			Table("chain_store_delay_bitrix_deals");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.LastUpdateDate).Column("last_update_date");
			Map(x => x.LastSynchronizedDate).Column("last_synchronized_date");
			Map(x => x.ClosedDate).Column("closed_date");
			Map(x => x.Status).Column("status");
			Map(x => x.BitrixDealId).Column("bitrix_deal_id");
			Map(x => x.LastError).Column("last_error");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.OrderId).Column("order_id");
			Map(x => x.DealTitle).Column("deal_title");
			Map(x => x.CounterpartyName).Column("counterparty_name").Length(1000);
			Map(x => x.CounterpartyInn).Column("counterparty_inn");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.DeliveryAddress).Column("delivery_address");
			Map(x => x.OrderAuthor).Column("order_author");
			Map(x => x.OrderSum).Column("order_sum");
			Map(x => x.EstimatedPaymentDate).Column("estimated_payment_date");
			Map(x => x.DaysOverdue).Column("days_overdue");
			Map(x => x.Debt).Column("debt");
		}
	}
}
