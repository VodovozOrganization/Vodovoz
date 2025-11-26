using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class InformalEdoRequestMap : ClassMap<InformalEdoRequest>
	{
		public InformalEdoRequestMap()
		{
			Table("edo_informal_requests");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("order_document_type");

			Map(x => x.Time)
				.Column("time")
				.ReadOnly();

			Map(x => x.Source)
				.Column("source");

			Map(x => x.DocumentType)
				.Column("document_type")
				.ReadOnly();

			Map(x => x.OrderDocumentType)
				.Column("order_document_type")
				.ReadOnly();

			References(x => x.Task)
				.Column("order_document_task_id")
				.Cascade.All()
				.Not.LazyLoad()
				.Unique();

			References(x => x.Order)
				.Column("order_id")
				.Cascade.All();
		}
	}
}
