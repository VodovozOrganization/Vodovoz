using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtMap : ClassMap<OrderWithoutShipmentForDebt>
	{
		public OrderWithoutShipmentForDebtMap()
		{
			Table("bills_without_shipment_for_debt");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.DebtName).Column("debt_name");
			Map(x => x.DebtSum).Column("debt_sum");
			Map(x => x.IncludeNDS).Column("include_nds");
			Map(x => x.ValueAddedTax).Column("value_added_tax");
			Map(x => x.IsBillWithoutShipmentSent).Column("is_bill_sent");

			References(x => x.Organization).Column("organization_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Client).Column("client_id");
		}
	}
}
