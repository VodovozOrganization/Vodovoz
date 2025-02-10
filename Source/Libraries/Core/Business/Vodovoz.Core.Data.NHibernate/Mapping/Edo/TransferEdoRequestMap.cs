using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoRequestMap : ClassMap<TransferEdoRequest>
	{
		public TransferEdoRequestMap()
		{
			Table("edo_transfer_requests");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Time)
				.Column("time")
				.ReadOnly();

			References(x => x.OrderEdoTask)
				.Column("document_edo_task_id")
				.Cascade.All();

			Map(x => x.FromOrganizationId)
				.Column("from_organization_id");

			Map(x => x.ToOrganizationId)
				.Column("to_organization_id");

			HasManyToMany(x => x.TransferedItems)
				.Table("edo_transfered_items")
				.ParentKeyColumn("transfer_edo_request_id")
				.ChildKeyColumn("customer_edo_task_item_id")
				.Cascade.AllDeleteOrphan();

			References(x => x.TransferEdoTask)
				.Column("transfer_edo_task_id")
				.Cascade.All();
		}
	}
}
