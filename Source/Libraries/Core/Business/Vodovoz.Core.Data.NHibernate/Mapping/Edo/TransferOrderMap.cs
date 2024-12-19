using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferOrderMap : ClassMap<TransferOrder>
	{
		public TransferOrderMap()
		{
			Table("transfer_orders");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.Seller)
				.Column("seller_organization_id");

			References(x => x.Customer)
				.Column("customer_organization_id");

			HasMany(x => x.TrueMarkCodes).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("transfer_order_id");
		}
	}
}
