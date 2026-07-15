using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PlannedOrderMap : ClassMap<PlannedOrder>
	{
		public PlannedOrderMap()
		{
			Table("planned_orders");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreationDate).Column("creation_date");
			Map(x => x.Stage).Column("stage");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.DeliveryPointId).Column("delivery_point_id");
			Map(x => x.IsSelfDelivery).Column("is_self_delivery");
			Map(x => x.CounterpartyName).Column("counterparty_name");
			Map(x => x.CounterpartyInn).Column("counterparty_inn");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.EmailAddress).Column("email_address");
			Map(x => x.DeliveryPointAddress).Column("delivery_point_address");
			Map(x => x.LastOrderDeliveryDate).Column("last_order_delivery_date");
			Map(x => x.PlannedOrderDate).Column("planned_order_date");
			Map(x => x.LastOrderBottlesCount).Column("last_order_bottles_count");
			Map(x => x.BottlesDebtByAddress).Column("bottles_debt_by_address");
			Map(x => x.BottlesDebtByCounterparty).Column("bottles_debt_by_counterparty");
			Map(x => x.DelayDaysForCounterparty).Column("delay_days_for_counterparty");
			Map(x => x.DebtorDebt).Column("debtor_debt");
		}
	}
}
