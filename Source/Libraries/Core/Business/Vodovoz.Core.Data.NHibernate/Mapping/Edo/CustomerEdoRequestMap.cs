using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class CustomerEdoRequestMap : ClassMap<FormalEdoRequest>
	{
		public CustomerEdoRequestMap()
		{
			Table("edo_customer_requests");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("request_type");

			Map(x => x.Type)
				.Column("type")
				.ReadOnly();

			Map(x => x.Time)
				.Column("time")
				.ReadOnly();

			Map(x => x.Source)
				.Column("source");

			Map(x => x.DocumentType)
				.Column("document_type");

			HasMany(x => x.ProductCodes)
				.KeyColumn("customer_request_id")
				.Cascade.AllDeleteOrphan();

			References(x => x.Order)
				.Column("order_id");

			References(x => x.Task)
				.Column("order_task_id")
				.Cascade.All()
				.Unique();
		}
	}
}
