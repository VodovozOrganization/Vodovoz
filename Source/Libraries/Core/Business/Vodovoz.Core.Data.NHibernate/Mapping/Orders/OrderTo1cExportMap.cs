using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OrderTo1cExportMap : ClassMap<OrderTo1cExport>
	{
		public OrderTo1cExportMap()
		{
			Table("orders_to_1c_exports");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.LastOrderChangeDate).Column("last_order_change_date");
			Map(x => x.LastExportDate).Column("last_export_date");
			Map(x => x.Error).Column("export_error");

			Map(x => x.OrderId)
				.Column("order_id")
				.Not.Nullable()
				.ReadOnly();

			References(x => x.Order)
				.Column("order_id")
				.NotFound.Ignore()
				.LazyLoad();
		}
	}
}
