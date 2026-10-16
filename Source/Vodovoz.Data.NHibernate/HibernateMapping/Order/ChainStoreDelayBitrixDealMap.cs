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
			Map(x => x.Status).Column("status");
			Map(x => x.BitrixDealId).Column("bitrix_deal_id");
			Map(x => x.LastError).Column("last_error");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.InitiatorOrderId).Column("initiator_order_id");
			Map(x => x.DealTitle).Column("deal_title");
			Map(x => x.CounterpartyName).Column("counterparty_name").Length(1000);
			Map(x => x.CounterpartyInn).Column("counterparty_inn");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.DeliveryAddresses).Column("delivery_addresses");
			Map(x => x.OrderAuthors).Column("order_authors");
			Map(x => x.OrdersSum).Column("orders_sum");
			Map(x => x.InitiatorEstimatedPaymentDate).Column("initiator_estimated_payment_date");
			Map(x => x.DaysOverdue).Column("days_overdue");
			Map(x => x.DebtSum).Column("debt_sum");

			HasMany(x => x.Orders)
				.KeyColumn("chain_store_delay_bitrix_deal_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}
