using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OrderDepositItemMap : ClassMap<OrderDepositItemEntity>
	{
		public OrderDepositItemMap()
		{
			Table("order_deposit_items");
			Not.LazyLoad();

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Deposit)
				.Column("deposit_sum");

			Map(x => x.Count)
				.Column("count");

			Map(x => x.ActualCount)
				.Column("actual_count");

			Map(x => x.DepositType)
				.Column("deposit_type");

			References(x => x.Order)
				.Column("order_id");
		}
	}
}
