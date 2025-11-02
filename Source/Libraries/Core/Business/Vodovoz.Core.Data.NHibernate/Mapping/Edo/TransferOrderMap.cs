using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferOrderMap : ClassMap<TransferOrder>
	{
		public TransferOrderMap()
		{
			Table("edo_transfer_orders");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Date)
				.Column("date");

			References(x => x.Seller)
				.Column("seller_organization_id");

			References(x => x.Customer)
				.Column("customer_organization_id");

			HasMany(x => x.Items)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("transfer_order_id");
		}
	}
}
