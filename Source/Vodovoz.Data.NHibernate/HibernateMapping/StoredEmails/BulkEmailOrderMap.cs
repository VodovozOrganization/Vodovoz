using FluentNHibernate.Mapping;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.Data.NHibernate.HibernateMapping.StoredEmails
{
	public class BulkEmailOrderMap : ClassMap<BulkEmailOrder>
	{
		public BulkEmailOrderMap()
		{
			Table("bulk_email_orders");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.Order)
				.Column("order_id")
				.Not.Nullable()
				.Unique();

			References(x => x.BulkEmail)
				.Column("bulk_email_id")
				.Not.Nullable();
		}
	}
}
